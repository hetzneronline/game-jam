"""runs on server"""
from flask import Flask, request, jsonify, abort
import requests
import time
import hmac
import hashlib
import base64
from cryptography.fernet import Fernet
import os

app = Flask(__name__)
OLLAMA_URL = "http://localhost:11434/api/generate"

# Generate a key if it doesn't exist, or load it
KEY_PATH = os.path.join(os.path.dirname(__file__), "secret_key.key")
if not os.path.exists(KEY_PATH):
    key = Fernet.generate_key()
    with open(KEY_PATH, 'wb') as key_file:
        key_file.write(key)
else:
    with open(KEY_PATH, 'rb') as key_file:
        key = key_file.read()

API_KEY = key.decode()
TOKEN_EXPIRY = 300  # 5 minutes in seconds

def verify_request_signature(request_data, timestamp, signature):
    """Verify request signature using HMAC"""
    if abs(int(time.time()) - int(timestamp)) > TOKEN_EXPIRY:
        return False
    
    # Convert request data to bytes for hashing
    data = str(request_data).encode('utf-8')
    message = data + timestamp.encode('utf-8')
    
    # Create expected signature
    expected_signature = hmac.new(
        API_KEY.encode('utf-8'), 
        message, 
        hashlib.sha256
    ).hexdigest()
    
    # Compare signatures using constant-time comparison
    return hmac.compare_digest(expected_signature, signature)

@app.route('/ask', methods=['POST'])
def ask_model():
    # Get authentication headers
    auth_timestamp = request.headers.get('X-Auth-Timestamp')
    auth_signature = request.headers.get('X-Auth-Signature')
    
    # Verify authentication
    if not auth_timestamp or not auth_signature:
        return jsonify({"error": "Authentication required"}), 401
    
    # Verify signature
    if not verify_request_signature(request.json, auth_timestamp, auth_signature):
        return jsonify({"error": "Invalid signature or expired timestamp"}), 403
    
    # Process legitimate request
    data = request.json
    prompt = data.get('prompt')
    model = data.get('model', 'llama3:8b')

    payload = {
        "model": model,
        "prompt": prompt,
        "stream": False
    }

    response = requests.post(OLLAMA_URL, json=payload)
    return jsonify(response.json())

if __name__ == '__main__':
    print(f"Server started with API key: {API_KEY[:10]}...")
    app.run(host='0.0.0.0', port=5000)