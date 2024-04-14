from flask import Flask, request, jsonify, send_file
from flask_cors import CORS 
import requests
import subprocess
from google.cloud import texttospeech, speech
import os
import base64
import nltk
from text2emotion import get_emotion



app = Flask(__name__)
CORS(app)  # Enable CORS for the entire app

nltk.download('punkt')
nltk.download('wordnet')
nltk.download('omw-1.4')
nltk.download('stopwords')


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
def generate():
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
            "maxOutputTokens": 1024,
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


def synthesize_speech(text, gender):
    client = texttospeech.TextToSpeechClient()
    synthesis_input = texttospeech.SynthesisInput(text=text)
    voice = texttospeech.VoiceSelectionParams(language_code="en-US", ssml_gender=gender)
    audio_config = texttospeech.AudioConfig(audio_encoding=texttospeech.AudioEncoding.LINEAR16)
    response = client.synthesize_speech(input=synthesis_input, voice=voice, audio_config=audio_config)
    return base64.b64encode(response.audio_content).decode('utf-8')

@app.route('/synthesize_speech_base64', methods=['POST'])
def synthesize_speech_base64():
    data = request.get_json()
    text = data.get('text')
    audio_content_base64 = synthesize_speech(text, texttospeech.SsmlVoiceGender.FEMALE)
    return jsonify({"audioContentBase64": audio_content_base64})

@app.route('/synthesize_speech_base64_male', methods=['POST'])
def synthesize_speech_base64_male():
    data = request.get_json()
    text = data.get('text')
    audio_content_base64 = synthesize_speech(text, texttospeech.SsmlVoiceGender.MALE)
    return jsonify({"audioContentBase64": audio_content_base64})

@app.route('/recognize_speech', methods=['POST'])
def recognize_speech():
    data = request.get_json()
    audio_content_base64 = data.get('audioContentBase64')
    
    audio_content = base64.b64decode(audio_content_base64)

    client = speech.SpeechClient()
    audio = speech.RecognitionAudio(content=audio_content)
    # Ensure this matches the sample rate of your audio
    config = speech.RecognitionConfig(
        encoding=speech.RecognitionConfig.AudioEncoding.LINEAR16,
        sample_rate_hertz=16000,  # Adjusted to likely sample rate
        language_code="en-US"
    )

    try:
        response = client.recognize(config=config, audio=audio)
        if response.results:
            recognized_text = response.results[0].alternatives[0].transcript
        else:
            recognized_text = "No speech recognized."
    except Exception as e:
        return jsonify({"error": str(e)}), 500

    return jsonify({"recognizedText": recognized_text})

@app.route('/analyze_emotions', methods=['POST'])
def analyze_text():
    text = request.get_json().get('text')
    if not text:
        return jsonify({'error': 'No text provided.'}), 400

    emotions = get_emotion(text)
    return jsonify(emotions), 200


if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5002)

