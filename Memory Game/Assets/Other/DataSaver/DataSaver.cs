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

		public List<UserWordPackProgress> wordPackData = new List<UserWordPackProgress>();
	}
	
	
	[Serializable]
	public class UserWordPackProgress {
		public string wordPackName;
		public List<UserWordPairProgress> wordPairData = new List<UserWordPairProgress>();

		public UserWordPairProgress GetWordPairData (int index) {
			if (wordPairData.Count > index) {
				return wordPairData[index];
			} else {
				while (wordPairData.Count <= index) {
					wordPairData.Add(new UserWordPairProgress());
				}
				return wordPairData[index];
			}
		}
	}
	
	[Serializable]
	public class UserWordPairProgress {
		public int type = 0; // 0=new, 1=learning, 2=review, 3=relearning
	}


}
