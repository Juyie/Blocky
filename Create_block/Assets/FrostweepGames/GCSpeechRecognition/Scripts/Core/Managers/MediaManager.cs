﻿using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using FrostweepGames.Plugins.Core;
using FrostweepGames.Plugins.Native;
using System.Collections;

namespace FrostweepGames.Plugins.GoogleCloud.SpeechRecognition
{
    public class MediaManager : IService, IMediaManager
    {
		public event Action MicrophoneDeviceSelectedEvent;

		public event Action RecordStartedEvent;
        public event Action RecordFailedEvent;
		public event Action<AudioClip, float[]> RecordEndedEvent;

		public event Action TalkBeganEvent;
        public event Action<AudioClip, float[]> TalkEndedEvent;

		private IVoiceDetectionManager _voiceDetectionManager;

		private ISpeechRecognitionManager _speechRecognitionManager;

		private AudioClip _microphoneWorkingAudioClip;

        private int _currentSamplePosition;

        private int _previousSamplePosition;

        private float[] _currentAudioSamples;

        private bool _isTalking;

        private List<float> _currentRecordingVoice;

		private float _maxVoiceFrame;

		private float _endTalkingDelay;

        public bool IsRecording { get; private set; }
		public string MicrophoneDevice { get; private set; }
		public AudioClip LastRecordedClip { get; private set; }
		public float[] LastRecordedRaw { get; private set; }
		public bool DetectVoice { get; private set; }

		public void Init()
        {
			_voiceDetectionManager = ServiceLocator.Get<IVoiceDetectionManager>();
			_speechRecognitionManager = ServiceLocator.Get<ISpeechRecognitionManager>();
		}

		public void Update()
        {
            if (IsRecording)
            {
                _currentSamplePosition = CustomMicrophone.GetPosition(MicrophoneDevice);
				CustomMicrophone.GetRawData(ref _currentAudioSamples, _microphoneWorkingAudioClip);

				if (DetectVoice)
                {
                    bool isTalking = _voiceDetectionManager.HasDetectedVoice(_currentAudioSamples);

					if (isTalking)
					{
						_endTalkingDelay = 0f;	
					}
					else
					{
						_endTalkingDelay += Time.deltaTime;
					}

                    if (!_isTalking && isTalking)
                    {
						AddStartAudioSamplesIntoBuffer();

						_isTalking = true;

						TalkBeganEvent?.Invoke();
					}
					else if (_isTalking && !isTalking && _endTalkingDelay >= _speechRecognitionManager.CurrentConfig.voiceDetectionEndTalkingDelay)
                    {
						_isTalking = false;

						LastRecordedRaw = _currentRecordingVoice.ToArray();
						LastRecordedClip = AudioConvert.Convert(LastRecordedRaw, _microphoneWorkingAudioClip.channels);

						_currentRecordingVoice.Clear();

						TalkEndedEvent?.Invoke(LastRecordedClip, LastRecordedRaw);
					}
                    else if (_isTalking && isTalking)
                    {
                        AddAudioSamplesIntoBuffer();
                    }
                }
                else
                {
                    AddAudioSamplesIntoBuffer();
                }

                _previousSamplePosition = _currentSamplePosition;
            }
        }

        public void Dispose()
        {
			if (_microphoneWorkingAudioClip != null)
			{
				MonoBehaviour.Destroy(_microphoneWorkingAudioClip);
				_microphoneWorkingAudioClip = null;
			}

			if (LastRecordedClip != null)
			{
				MonoBehaviour.Destroy(LastRecordedClip);
				LastRecordedClip = null;
			}
        }

		public float GetLastFrame()
		{
			int minValue = 16000 / 8;

			if (_currentRecordingVoice == null || _currentRecordingVoice.Count < minValue)
				return 0;

			int position = Mathf.Clamp(_currentRecordingVoice.Count - (minValue + 1), 0, _currentRecordingVoice.Count-1);

			float sum = 0f;
			for(int i = position; i < _currentRecordingVoice.Count; i++)
			{
				sum += Mathf.Abs(_currentRecordingVoice[i]);
			}

			sum /= minValue;

			return sum;
		}

		public float GetMaxFrame()
		{
			return _maxVoiceFrame;
		}

		public void StartRecord(bool withVoiceDetection = false)
		{
			if (IsRecording)
				return;

			if(!ReadyToRecord())
			{
				RecordFailedEvent?.Invoke();
				return;
			}

			DetectVoice = withVoiceDetection;

			_maxVoiceFrame = 0;

			_currentRecordingVoice = new List<float>();

			if (_microphoneWorkingAudioClip != null)
			{
				MonoBehaviour.Destroy(_microphoneWorkingAudioClip);
			}

			if (LastRecordedClip != null)
			{
				MonoBehaviour.Destroy(LastRecordedClip);
			}

			_microphoneWorkingAudioClip = CustomMicrophone.Start(MicrophoneDevice, true, 1, 16000);

			_currentAudioSamples = new float[_microphoneWorkingAudioClip.samples * _microphoneWorkingAudioClip.channels];

			IsRecording = true;

			RecordStartedEvent?.Invoke();
		}

		public void StopRecord()
		{
			if (!IsRecording || !ReadyToRecord())
				return;

			IsRecording = false;

			CustomMicrophone.End(MicrophoneDevice);

			if (!DetectVoice)
			{
				LastRecordedRaw = _currentRecordingVoice.ToArray();
				LastRecordedClip = AudioConvert.Convert(LastRecordedRaw, _microphoneWorkingAudioClip.channels);			
			} 
			else
			{
				LastRecordedRaw = null;
				LastRecordedClip = null;
			}

			if (_currentRecordingVoice != null)
			{
				_currentRecordingVoice.Clear();
			}

			_currentAudioSamples = null;
			_currentRecordingVoice = null;

			RecordEndedEvent?.Invoke(LastRecordedClip, LastRecordedRaw);
		}

		public bool ReadyToRecord()
		{
			return HasConnectedMicrophoneDevices() && !string.IsNullOrEmpty(MicrophoneDevice);
		}

		public bool HasConnectedMicrophoneDevices()
		{
			return CustomMicrophone.HasConnectedMicrophoneDevices();
		}

		public void SetMicrophoneDevice(string device)
		{
			if(MicrophoneDevice == device)
			{
				Debug.LogWarning("you are trying to select microphone device that already selected");
				return;
			}

			MicrophoneDevice = device;

			MicrophoneDeviceSelectedEvent?.Invoke();
		}

		public string[] GetMicrophoneDevices()
		{
			return CustomMicrophone.devices;
		}

		public void SaveLastRecordedAudioClip(string path)
		{
			if (LastRecordedClip != null)
			{
				try
				{
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID || UNITY_IPHONE
					File.WriteAllBytes(path, AudioClip2PCMConverter.AudioClip2PCM(LastRecordedClip));
#endif
				}
				catch (Exception e)
				{
					Debug.LogException(e);
				}
			}
		}


		public IEnumerator OneTimeRecord(int durationSec, Action<float[]> callback, int sampleRate = 16000)
		{
			AudioClip clip = CustomMicrophone.Start(MicrophoneDevice, false, durationSec, sampleRate);

			yield return new WaitForSeconds(durationSec);

			CustomMicrophone.End(MicrophoneDevice);

			float[] array = new float[clip.samples * clip.channels];

			CustomMicrophone.GetRawData(ref array, clip);

			callback?.Invoke(array);
		}

		public bool HasMicrophonePermission()
		{
			return CustomMicrophone.HasMicrophonePermission();
		}

		/// <summary>
		/// Currently works as synchronous function with callback when app unpauses
		/// could not work properly if has enabled checkbox regarding additional frame in pause
		/// </summary>
		/// <param name="callback"></param>
		public void RequestMicrophonePermission(Action<bool> callback)
		{
			CustomMicrophone.RequestMicrophonePermission();
			callback?.Invoke(HasMicrophonePermission());
		}

		private void AddAudioSamplesIntoBuffer()
		{
			if (_previousSamplePosition > _currentSamplePosition)
			{
				for (int i = _previousSamplePosition; i < _currentAudioSamples.Length; i++)
				{
					_currentRecordingVoice.Add(_currentAudioSamples[i]);

					if (_currentAudioSamples[i] > _maxVoiceFrame)
						_maxVoiceFrame = _currentAudioSamples[i];
				}

				_previousSamplePosition = 0;
			}

			for (int i = _previousSamplePosition; i < _currentSamplePosition; i++)
			{
				_currentRecordingVoice.Add(_currentAudioSamples[i]);

				if (_currentAudioSamples[i] > _maxVoiceFrame)
					_maxVoiceFrame = _currentAudioSamples[i];
			}

			_previousSamplePosition = _currentSamplePosition;
		}

		private void AddStartAudioSamplesIntoBuffer()
		{
			int count = _currentSamplePosition - 2000;

			if (count >= 0)
			{
				for (int i = count; i < _currentSamplePosition; i++)
				{
					_currentRecordingVoice.Add(_currentAudioSamples[i]);

					if (_currentAudioSamples[i] > _maxVoiceFrame)
						_maxVoiceFrame = _currentAudioSamples[i];
				}
			}
			else
			{
				for (int i = _currentAudioSamples.Length - Mathf.Abs(count); i < _currentAudioSamples.Length; i++)
				{
					_currentRecordingVoice.Add(_currentAudioSamples[i]);

					if (_currentAudioSamples[i] > _maxVoiceFrame)
						_maxVoiceFrame = _currentAudioSamples[i];
				}

				for (int i = 0; i < _currentSamplePosition; i++)
				{
					_currentRecordingVoice.Add(_currentAudioSamples[i]);

					if (_currentAudioSamples[i] > _maxVoiceFrame)
						_maxVoiceFrame = _currentAudioSamples[i];
				}
			}

			_previousSamplePosition = _currentSamplePosition;
		}
	}
}