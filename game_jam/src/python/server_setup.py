#!/usr/bin/env python3
# filepath: h:\GameJam2025\src\python\server_setup.py
"""
Setup script for the Flask server and Tailscale connection
This script helps set up the server environment for the game's LLM integration
"""

import os
import sys
import subprocess
import platform
import json

CURRENT_DIR = os.path.dirname(os.path.abspath(__file__))
REQUIREMENTS_FILE = os.path.join(CURRENT_DIR, "requirements.txt")
CONFIG_FILE = os.path.join(CURRENT_DIR, "config.json")


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


def generate_api_key():
    """Generate API key for server-client communication"""
    from cryptography.fernet import Fernet

    key = Fernet.generate_key()
    secret_key_path = os.path.join(CURRENT_DIR, "secret_key.key")
    client_key_path = os.path.join(CURRENT_DIR, "client_key.key")

    # Save server key
    with open(secret_key_path, 'wb') as key_file:
        key_file.write(key)

    # Save client key
    with open(client_key_path, 'wb') as key_file:
        key_file.write(key)

    print(f"✓ API key generated:")
    print(f"  - Server key saved to {secret_key_path}")
    print(f"  - Client key saved to {client_key_path}")
    print(f"  - First 10 characters of key: {key[:10].decode()}")


def setup_tailscale():
    """Guide user through Tailscale setup"""
    system = platform.system().lower()

    print("\nTailscale Setup Instructions:")
    if system == "windows":
        print(
            "\033[93m1. Download Tailscale from: https://tailscale.com/download/windows")
        print("2. Run the installer and follow the prompts")
        print("3. Sign in to Tailscale when prompted\033[0m")
    elif system == "linux":
        print("\033[93m1. Install Tailscale:")
        print("   curl -fsSL https://tailscale.com/install.sh | sh")
        print("2. Start Tailscale and authenticate:")
        print("   sudo tailscale up\033[0m")
        if "y" == input("Should I run the install command for you? (y/n): ").lower():
            print("Running Tailscale install command...")
            subprocess.run(
                ["curl", "-fsSL", "https://tailscale.com/install.sh", "|", "sh"])
            subprocess.run(["sudo", "tailscale", "up"])
    else:
        print(f"Unsupported system: {system}")
        print(
            "\033[93mPlease visit https://tailscale.com/download for installation instructions\033[0m")

    print("\nAfter installing and signing in to Tailscale:")
    print("\033[93m1. Get your Tailscale IP from the Tailscale admin console")
    print(
        "2. Update the TAILSCALE_IP variable in query_llm.py with your IP\033[0m")

    ip = input("\nEnter your Tailscale IP (or press Enter to do this later): ")
    if ip:
        # update_tailscale_ip(ip)
        # llm_file =
        print(f"Please update the TAILSCALE_IP variable in query_llm.py with {ip}\n",
              os.path.join(CURRENT_DIR, "query_llm.py"))


def update_tailscale_ip(ip):
    """Update the Tailscale IP in the query_llm.py file"""
    llm_file = os.path.join(CURRENT_DIR, "query_llm.py")

    if not os.path.exists(llm_file):
        print("Warning: query_llm.py not found, cannot update Tailscale IP")
        return

    with open(llm_file, 'r') as f:
        content = f.read()

    # Replace the Tailscale IP
    lines = []
    found = False
    with open(llm_file, 'r') as f:
        for line in f.readlines():
            if line.strip().startswith("TAILSCALE_IP ="):
                lines.append(f'TAILSCALE_IP = "{ip}"\n')
                found = True
            else:
                lines.append(line)

    if not found:
        print("Warning: Could not find TAILSCALE_IP variable in query_llm.py")
        return

    with open(llm_file, 'w') as f:
        f.writelines(lines)

    print(f"✓ Updated Tailscale IP to {ip} in query_llm.py")


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


def run_server_test():
    """Run a test of the server"""
    print("\nTesting server...")

    api_file = os.path.join(CURRENT_DIR, "ollama_api.py")
    if not os.path.exists(api_file):
        print("Warning: ollama_api.py not found, cannot test server")
        return

    try:
        print("Starting Flask server (press Ctrl+C to stop)...")
        subprocess.run([sys.executable, api_file])
    except KeyboardInterrupt:
        print("\nServer test stopped.")


def main():
    """Main setup function"""
    print("==== Escape Room 2025 - Flask Server & Tailscale Setup ====\n")

    check_python_version()
    install_requirements()
    generate_api_key()
    setup_tailscale()
    save_config()

    print("\nSetup complete! You can now run the following commands:")
    print(
        f"1. Start the server: python {os.path.join('src', 'python', 'ollama_api.py')}")
    print(
        f"2. Test the client: python {os.path.join('src', 'python', 'query_llm.py')}")

    test = input("\nWould you like to test the server now? (y/n): ")
    if test.lower() == 'y':
        run_server_test()


if __name__ == "__main__":
    main()
