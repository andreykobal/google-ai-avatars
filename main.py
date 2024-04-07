from flask import Flask, request, jsonify
import requests
import subprocess

app = Flask(__name__)

def get_access_token():
    """Runs the gcloud command to get the access token and returns it."""
    try:
        # Run the gcloud command to get the access token
        completed_process = subprocess.run(["gcloud", "auth", "print-access-token"], check=True, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
        # Extract the access token from the command output
        access_token = completed_process.stdout.strip()
        return access_token
    except subprocess.CalledProcessError as e:
        print(f"Error getting access token: {e}")
        return None

@app.route('/generate', methods=['POST'])
def generate_poem():
    # Extract prompt from incoming request
    data = request.get_json()
    prompt = data.get('prompt')

    # Prepare data for the API request
    api_request_data = {
        "instances": [
            {"prompt": prompt}
        ],
        "parameters": {
            "temperature": 0.8,
            "maxOutputTokens": 100,
            "topP": 0.9,
            "topK": 40
        }
    }

    # Get the access token using the gcloud command
    access_token = get_access_token()
    if not access_token:
        return jsonify({"error": "Failed to get access token"}), 500

    # Set the URL and headers for the API request
    api_url = "https://us-central1-aiplatform.googleapis.com/v1/projects/ailand-testnet/locations/us-central1/publishers/google/models/text-bison:predict"
    headers = {
        "Authorization": f"Bearer {access_token}",
        "Content-Type": "application/json; charset=utf-8"
    }

    # Make the API request
    response = requests.post(api_url, headers=headers, json=api_request_data)

    # Check for a successful response
    if response.status_code == 200:
        response_data = response.json()
        # Extract the poem content from the response
        poem_content = response_data['predictions'][0]['content']
        return jsonify({"response": poem_content})
    else:
        return jsonify({"error": "Failed to generate poem"}), response.status_code

if __name__ == '__main__':
    app.run(debug=True, port=5002)
