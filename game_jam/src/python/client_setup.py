#!/usr/bin/env python3
# filepath: h:\GameJam2025\src\python\client_setup.py
"""
Setup script for the client connection to the Flask server
This script helps set up the client environment for the game's LLM integration
"""

import os
import sys
import subprocess
import platform
import json

CURRENT_DIR = os.path.dirname(os.path.abspath(__file__))
REQUIREMENTS_FILE = os.path.join(CURRENT_DIR, "requirements.txt")
CONFIG_FILE = os.path.join(CURRENT_DIR, "client_config.json")
CLIENT_KEY_PATH = os.path.join(CURRENT_DIR, "client_key.key")


def check_python_version():
    """Check that Python version is 3.8+"""
    major, minor = sys.version_info[:2]
    if major < 3 or (major == 3 and minor < 8):
        print("Error: Python 3.8 or higher is required")
        sys.exit(1)
    print(f"✓ Python version {major}.{minor} detected")


def install_requirements():
    """Install required Python packages"""
    print("\nInstalling required packages...")
    try:
        subprocess.check_call(
            [sys.executable, "-m", "pip", "install", "-r", REQUIREMENTS_FILE])
        print("✓ Required packages installed successfully")
    except subprocess.CalledProcessError:
        print("Error: Failed to install required packages")
        sys.exit(1)


def setup_api_key():
    """Setup the API key for the client"""
    if os.path.exists(CLIENT_KEY_PATH):
        print("\n✓ API key already exists")
        print(f"  - Client key file: {CLIENT_KEY_PATH}")

        with open(CLIENT_KEY_PATH, 'rb') as key_file:
            key = key_file.read()
            print(f"  - First 10 characters of key: {key[:10].decode()}")
    else:
        print("\nAPI key not found. You need to get this from the server.")
        key = input("Enter the API key provided by the server: ")

        if key:
            with open(CLIENT_KEY_PATH, 'wb') as key_file:
                key_file.write(key.encode())
            print(f"✓ API key saved to {CLIENT_KEY_PATH}")
        else:
            print("Warning: No API key provided. You'll need to add it later.")


def setup_tailscale():
    """Guide user through Tailscale setup"""
    system = platform.system().lower()

    print("\nTailscale Setup Instructions:")

    if system == "windows":
        print("1. Download Tailscale from: https://tailscale.com/download/windows")
        print("2. Run the installer and follow the prompts")
        print("3. Sign in to Tailscale when prompted")
    elif system == "linux":
        print("1. Install Tailscale:")
        print("   curl -fsSL https://tailscale.com/install.sh | sh")
        print("2. Start Tailscale and authenticate:")
        print("   sudo tailscale up")
    else:
        print(f"Unsupported system: {system}")
        print("Please visit https://tailscale.com/download for installation instructions")

    print("\nAfter installing and signing in to Tailscale:")
    print("1. Make sure you're connected to the same Tailscale network as the server")
    print("2. The server's Tailscale IP should already be configured in query_llm.py")

    ip = input("\nEnter the server's Tailscale IP (or press Enter to skip): ")
    if ip:
        update_tailscale_ip(ip)


def update_tailscale_ip(ip):
    """Update the Tailscale IP in the query_llm.py file"""
    llm_file = os.path.join(CURRENT_DIR, "query_llm.py")

    if not os.path.exists(llm_file):
        print("Warning: query_llm.py not found, cannot update Tailscale IP")
        return

    with open(llm_file, 'r') as f:
        content = f.read()

    # Replace the Tailscale IP
    if "TAILSCALE_IP =" in content:
        new_content = content
        # Replace the line with the new IP
        import re
        new_content = re.sub(
            r'TAILSCALE_IP\s*=\s*"[^"]*"', f'TAILSCALE_IP = "{ip}"', new_content)
        new_content = re.sub(
            r"TAILSCALE_IP\s*=\s*'[^']*'", f'TAILSCALE_IP = "{ip}"', new_content)

        with open(llm_file, 'w') as f:
            f.write(new_content)

        print(f"✓ Updated Tailscale IP to {ip} in query_llm.py")
    else:
        print("Warning: Could not find TAILSCALE_IP variable in query_llm.py")


def save_config():
    """Save configuration to config.json"""
    if os.path.exists(CONFIG_FILE):
        with open(CONFIG_FILE, 'r') as f:
            config = json.load(f)
    else:
        config = {}

    config["setup_complete"] = True
    config["python_version"] = ".".join(map(str, sys.version_info[:3]))
    config["system"] = platform.system()

    with open(CONFIG_FILE, 'w') as f:
        json.dump(config, f, indent=2)

    print(f"\n✓ Configuration saved to {CONFIG_FILE}")


def run_client_test():
    """Run a test of the client"""
    print("\nTesting client...")

    client_file = os.path.join(CURRENT_DIR, "query_llm.py")
    if not os.path.exists(client_file):
        print("Warning: query_llm.py not found, cannot test client")
        return

    try:
        subprocess.run([sys.executable, client_file, "--test"])
    except Exception as e:
        print(f"Error testing client: {e}")


def main():
    """Main setup function"""
    print("==== Escape Room 2025 - Client Setup ====\n")

    check_python_version()
    install_requirements()
    setup_api_key()
    setup_tailscale()
    save_config()

    print("\nSetup complete! You can now run the client with:")
    print(f"python {os.path.join('src', 'python', 'query_llm.py')}")

    test = input("\nWould you like to test the client connection now? (y/n): ")
    if test.lower() == 'y':
        run_client_test()


if __name__ == "__main__":
    main()
