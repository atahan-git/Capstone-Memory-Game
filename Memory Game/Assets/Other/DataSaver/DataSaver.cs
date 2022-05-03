using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FullSerializer;

[Serializable]
public class DataSaver {

	public static DataSaver s;

	[SerializeField]
	private SaveFile activeSave;
	public const string saveName = "save.data";

	public bool loadingComplete = false;
	
	private static readonly fsSerializer _serializer = new fsSerializer();

	public string GetSaveFilePathAndFileName () {
		return Application.persistentDataPath + "/" + saveName;
	}

	public delegate void SaveYourself ();
	public static event SaveYourself earlyLoadEvent;
	public static event SaveYourself loadEvent;
	public static event SaveYourself earlySaveEvent;
	public static event SaveYourself saveEvent;


	public SaveFile GetCurrentSave() {
		return activeSave;
	}

	public void ClearCurrentSave() {
		Debug.Log("Clearing Save");
		activeSave = MakeNewSaveFile();
	}
	
	public SaveFile MakeNewSaveFile() {
		var file = new SaveFile();
		PlayerLoadoutController.SetDefaultLoadoutWordPacks(file);
		file.isRealSaveFile = true;
		return file;
	}


	public bool dontSave = false;
	public void SaveActiveGame () {
		if (!dontSave) {
			earlySaveEvent?.Invoke();
			saveEvent?.Invoke();
			Save();
		}
	}

	void Save() {
		var path = GetSaveFilePathAndFileName();

		activeSave.isRealSaveFile = true;
		SaveFile data = activeSave;

		WriteFile(path, data);
	}

	public static void WriteFile(string path, object file) {
		Directory.CreateDirectory(Path.GetDirectoryName(path));
		
		StreamWriter writer = new StreamWriter(path);
		
		fsData serialized;
		_serializer.TrySerialize(file, out serialized);
		var json = fsJsonPrinter.PrettyJson(serialized);

		writer.Write(json);
		writer.Close();

		Debug.Log($"IO OP: file \"{file.GetType()}\" saved to \"{path}\"");
	}

	public void Load () {
		if (loadingComplete) {
			return;
		}

		var path = GetSaveFilePathAndFileName();
		try {
			if (File.Exists(path)) {
				activeSave = ReadFile<SaveFile>(path);
			} else {
				Debug.Log($"No Save Data Found");
				activeSave = MakeNewSaveFile();
			}
		} catch {
			File.Delete(path);
			Debug.Log("Corrupt Data Deleted");
			activeSave = MakeNewSaveFile();
		}

		earlyLoadEvent?.Invoke();
		loadEvent?.Invoke();
		loadingComplete = true;
	}

	public static T ReadFile<T>(string path) where T : class, new() {
		StreamReader reader = new StreamReader(path);
		var json = reader.ReadToEnd();
		reader.Close();

		fsData serialized = fsJsonParser.Parse(json);

		T file = new T();
		_serializer.TryDeserialize(serialized, ref file).AssertSuccessWithoutWarnings();

		Debug.Log($"IO OP: file \"{file.GetType()}\" read from \"{path}\"");
		return file;
	}


	[Serializable]
	public class SaveFile {
		public bool isRealSaveFile = false;
		public bool isDefaultWordPacksLoaded = false;

		public string[] loadoutWordPackNames = new string[3];

		[SerializeField]
		private List<UserWordPackProgress> wordPackData = new List<UserWordPackProgress>();
		
		public UserWordPackProgress GetProgress(WordPack wordPack) {
			var index = wordPackData.FindIndex((progress => progress.wordPackName == wordPack.wordPackName));
			if (index != -1) {
				return wordPackData[index];
			} else {
				var progress = new UserWordPackProgress();
				progress.wordPackName = wordPack.wordPackName;
				wordPackData.Add(progress);
				return progress;
			}
		}
	}
	
	
	[Serializable]
	public class UserWordPackProgress {
		public string wordPackName;
		public List<UserWordPairProgress> wordPairData = new List<UserWordPairProgress>();

		public UserWordPairProgress GetWordPairData (WordPair wordPair) {
			return GetWordPairData(wordPair.id);
		}
		UserWordPairProgress GetWordPairData (int wordPairId) {
			if (wordPairData.Count > wordPairId) {
				return wordPairData[wordPairId];
			} else {
				while (wordPairData.Count <= wordPairId) {
					wordPairData.Add(new UserWordPairProgress());
				}
				return wordPairData[wordPairId];
			}
		}
		
		public List<UserWordPairProgress> GetWordPairData () {
			return wordPairData;
		}
	}
	
	[Serializable]
	public class UserWordPairProgress {
		public int type = 0; // 0=new, 1=learning
		
		public long meaningSide_lastRecallUtcFiletime;
		public long foreignSide_lastRecallUtcFiletime;
		public int meaningSide_correctRecallCount;
		public int foreignSide_correctRecallCount;
		public int meaningSide_wrongRecallCount;
		public int foreignSide_wrongRecallCount;

		public long GetLastRecallUtcFileTime(bool isMeaningSide) {
			return isMeaningSide ? meaningSide_lastRecallUtcFiletime : foreignSide_lastRecallUtcFiletime;
		}

		public int GetCorrect(bool isMeaningSide) {
			return isMeaningSide ? meaningSide_correctRecallCount : foreignSide_correctRecallCount;
		}

		public int GetWrong(bool isMeaningSide) {
			return isMeaningSide ? meaningSide_wrongRecallCount : foreignSide_wrongRecallCount;
		}
		
		public void SetLastRecallUtcFileTime(bool isMeaningSide, long value) {
			if (isMeaningSide) {
				meaningSide_lastRecallUtcFiletime = value;
			} else {
				foreignSide_lastRecallUtcFiletime = value;
			}
		}

		public void Increment(bool isMeaningSide, bool isCorrect) {
			if (isCorrect) {
				if (isMeaningSide) {
					meaningSide_correctRecallCount += 1;
				} else {
					foreignSide_correctRecallCount += 1;
				}
			} else {
				if (isMeaningSide) {
					meaningSide_wrongRecallCount += 1;
				} else {
					foreignSide_wrongRecallCount += 1;
				}
			}
		}
	}
}
