 # Google AI Avatars: Revolutionizing NPC Interactions in Gaming and Web Apps

### Executive Summary
**Google AI Avatars: Next-Gen AI-Powered NPCs**

Empowering the future of interactive gaming and web applications, the Google AI Avatars SDK revolutionizes NPC interactions by leveraging advanced AI technologies. This innovative SDK enhances player immersion by enabling dynamic, realistic interactions with NPCs through text and voice, powered by Unity and Google Vertex AI. Addressing a widespread demand among gamers for intelligent NPC behavior, Google AI Avatars opens new possibilities for game narrative and user engagement across platforms

![ai avatars](https://github.com/andreykobal/google-ai-avatars/assets/19206978/468f8b6f-57ae-4c62-b94d-317c11093b12)

### 1. Demo 

- #### [Interactive Demo](https://ethernity.app/demo/)
- #### [Demo Video](https://example.com)

### 2. Problem Statement
In the gaming and web app industries, NPCs (non-player characters) often lack dynamic interactions, limiting player engagement and immersion. Research indicates that 99% of gamers believe that advanced AI in NPCs could significantly enhance their gaming experience, influencing both their time spent and willingness to pay more for intelligent NPC interactions.

Source: [Inworld study on the future of NPCs finds 99% of gamers think AI will enhance gameplay](https://inworld.ai/blog/future-of-npcs-report)

### 3. Solution Overview
**Google AI Avatars** is an SDK designed to integrate cutting-edge AI functionalities with NPCs in games and web apps, providing highly interactive and realistic experiences. This solution is engineered using Unity for the client-side and Google Vertex AI models on the backend to facilitate rich interactions through both text and voice.

#### Modes of Interaction
- **User <> NPC Interaction**:
  - Players can interact with NPCs using text or voice, benefitting from intelligent response suggestions.
  - Interactions are saved within a user-friendly chat UI, reminiscent of modern AI chatbots.

![ezgif-5-075e0129c0](https://github.com/andreykobal/google-ai-avatars/assets/19206978/1c24363b-35a7-42ef-8755-440bf8033907)

- **NPC <> NPC Interaction**:
  - Two NPCs can autonomously generate dialogues on set topics, mimicking real human conversations, complete with subtitles.

![ezgif-5-f570816c8c](https://github.com/andreykobal/google-ai-avatars/assets/19206978/bdc26488-c4b0-4bc1-ad97-0a2ed8df0785)

### 4. Technologies Used
- **Vertex AI API**: Utilizes language processing, text-to-speech, and speech recognition.
- **Programming**: Python, Flask.
- **Natural Language Processing**: NLTK, Text2Emotion for emotional analysis.
- **Deployment**: Hosted on Google Cloud to leverage robust and scalable cloud computing.
- **Game Development**: Unity for creating immersive client-side applications.

### 5. Features and Functionality
- **Interactive Chat**: Engage with AI-powered NPCs via text or voice.
- **Intelligent Response Suggestions**: AI generates contextually appropriate responses for users.
- **Realistic Conversations**: Watch NPCs converse with lifelike accuracy.
- **Advanced Animation**: Realistic eye movements and lip-syncing for NPCs.
- **Emotional Intelligence**: NPCs analyze and express emotions through facial expressions.
- **Developer-Friendly SDK**: Easy integration into cross-platform games and web applications.

### 6. Challenges Faced
Throughout the development, integrating real-time speech recognition and ensuring seamless interaction between different AI technologies were significant challenges. Our team overcame these by optimizing AI model responses and enhancing the synchronization between Unity and the backend APIs.

### 7. Accomplishments and Impact
We have successfully developed a versatile SDK that not only enriches user experience through enhanced NPC interactions but also opens up new possibilities for narrative depth in games and interactive applications. Our demo has been exceptionally well-received in user testing, with feedback praising the innovative approach to NPC realism.

### 8. Future Work
Looking forward, we plan to integrate this technology into "Ethernity," our upcoming AI-powered PvPvE shooter game. We aim to develop open-world and dynamic game modes that utilize "Gemini AI Pro," an advanced version of our SDK that allows NPCs to remember players' actions and maintain long-term chat histories, thereby enhancing personalized gameplay experiences.

# 9. SDK Documentation

This is a comprehensive guide for integrating and utilizing the Google AI Avatars SDK. This SDK facilitates the creation of next-gen AI-powered NPCs (non-player characters) for games and web applications, providing realistic interactions through advanced AI technologies.

## Getting Started

### Initial Setup

Before you begin, you will need to set up your environment to use Google Cloud's AI services. Refer to the following documentation to get started:

- **Generative AI**: [Vertex AI Generative AI Documentation](https://cloud.google.com/vertex-ai/generative-ai/docs/)
- **Text-to-Speech**: [Text-to-Speech Documentation](https://cloud.google.com/text-to-speech/docs/)
- **Speech-to-Text**: [Speech-to-Text Documentation](https://cloud.google.com/speech-to-text/docs)

### Installation

1. Clone the repository to your local machine.
2. Ensure you have Python and Flask installed.
3. Install necessary Python libraries as mentioned in `requirements.txt`.
4. Set up Google Cloud credentials by following the linked documentation.

## Server-Side Application

The Flask application acts as the backend for the Google AI Avatars SDK. Below are the endpoints provided by the `main.py` script:

### Endpoints

- **POST /generate**: Generates responses based on user input using AI.
  - Input: JSON with 'prompt'
  - Output: AI-generated text based on the prompt

- **POST /synthesize_speech_base64**: Converts text to speech and returns audio in base64 encoding.
  - Input: JSON with 'text'
  - Output: Base64 encoded speech audio

- **POST /synthesize_speech_base64_male**: Similar to the above but uses a male voice.
  - Input: JSON with 'text'
  - Output: Base64 encoded speech audio

- **POST /recognize_speech**: Converts speech audio back to text.
  - Input: JSON with 'audioContentBase64'
  - Output: Recognized text

- **POST /analyze_emotions**: Analyzes the emotional content of the text.
  - Input: JSON with 'text'
  - Output: Emotion analysis results

### Running the Server

To run the server:
```bash
python main.py
```
This will start the Flask server on `localhost` at port `5002`.

## Unity Project Setup

Located in `/unity-project` directory, this project contains several scripts that integrate the SDK's functionalities into Unity applications.

### Scripts and Their Functions

- **`AudioAnimationController.cs`**: Manages animation of avatars based on audio.
- **`AudioResampler.cs`**: Resamples WAV files to ensure compatibility.
- **`AvatarSelector.uss`**: Manages styles for the UI elements in the game.
- **`BlendShapeSync.cs`**: Synchronizes facial blendshapes for realistic animations.
- **`ChatHistoryManager.cs`**: Manages the display and storage of chat history in the UI.
- **`EmotionAnalyzer.cs`**: Requests and retrieves emotion analysis results.
- **`EmotionManager.cs`**: Adjusts avatar facial expressions based on analyzed emotions.
- **`MicrophoneRecorder.cs`**: Handles microphone input in WebGL.
- **`NPCManager.cs`**: Generates and manages NPC conversations.
- **`SceneChanger.cs`**: Manages transitions between different scenes in Unity.
- **`SpeechRecognitionManager.cs`**: Handles speech-to-text functionality.
- **`SubtitleDisplayManager.cs`**: Displays subtitles for spoken dialogues.
- **`SuggestionsGeneratorClient.cs`**: Generates AI-based suggestions for user inputs.
- **`TextGeneratorClient.cs`**: Main client for generating responses from AI.
- **`TextToSpeechClient.cs`**: Converts text to speech within the game.
- **`WavUtility.cs`**: Utilities for handling WAV file conversions.
- **`WebRequestUtility.cs`**: Facilitates web requests within Unity.

### Building the Project

To build the Unity project for all supported platforms:
1. Open the Unity Editor.
2. Load the project from the `/unity-project` directory.
3. Build the project using Unity's build tools.

## Deployment

Deploy the Flask application using your preferred method:
- **Containerization**: Use Docker to containerize the Flask app.
- **Cloud Deployment**: Deploy using Google Cloud VMs or App Engine for scalability and ease of management.

### 10. Team Name and Members
**Team ETHRNITY**:

- **Valentin Sotov, CEO**
  - 20+ years in entrepreneurship with 2 successful exits
  - Lifelong gamer and streamer
  - Drives strategic vision for the company

- **Andrew Kobal, CTO**
  - 10+ years of expertise in tech and AI/ML
  - Former Dota 2 eSports player
  - Brings 25 years of gaming experience to technical forefront

- **Demetre Shonia, Lead Developer**
  - 5+ years in the gaming industry
  - Spearheads game development with innovation
  - Expertise in game development technologies

- **Valeriia Kostenetska, Head of UX/UI**
  - Extensive knowledge in UI/UX design
  - Passionate artist enhancing player interaction
  - Proficient in intuitive Web3 technologies

### Achievements
- 🏆 Game Development World Championship: Best Web3 Game Finalist
- 🥇 BNB Chain 2024 Q1 Hackathon Winner
- 🥇 Klaymakers 2023 Global Hackathon Winner
- 🥇 Bitcoin Olympics Hackathon Winner
- 🥇 Polygon DevX Hackathon Winner
- 🥇 GameWave Genesis Hackathon Winner
- 🥇 SAGA Multiverse Hackathon Winner
- 🥇 ZetaChain Omnichain Hackathon Winner
- 🥇 Kirobu Trading Hackathon Winner
- 🥇 AIBC Playnance Hackathon Winner
- 🥇 Hack-a-TONx Winner


### 11. Conclusion
**Google AI Avatars** stands at the forefront of a revolution in digital interaction within the gaming and web app industries. By bridging the gap between technology and user experience, this project paves the way for a new era of engagement where every interaction feels uniquely personal and profoundly engaging. The **SDK** is designed to empower developers to create more engaging and realistic interactions in games and web apps. 


### 12. Resources
- **📌 [Pitch Deck](https://docsend.com/view/kmasga75mhhgiqcc)**
- **📹 [Game Trailer](https://www.youtube.com/watch?v=QkMLtXndIiE)**
- **🌐 [Website](https://ethernity.app/)**
- **👾 [Discord](https://discord.com/invite/5ze32SFmmS)** 
- **🐦 [Twitter](https://twitter.com/0xETHERNITY)**
- **📜 [Medium](https://medium.com/@0xETHERNITY)**
