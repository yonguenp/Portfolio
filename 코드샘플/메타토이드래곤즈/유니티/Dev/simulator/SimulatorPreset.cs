using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

#if DEBUG

namespace SandboxNetwork
{
	public enum ePreset
	{
		DRAGON,
		PART,
		PET,
	}

	public class JsonPreset
	{
		protected ePreset pType;
		public ePreset PresetType { get { return pType; } /*set*/ }

		protected int pid;
		public int PresetID { get { return pid; } /*set*/ }

		protected string name;
		public string PresetName { get { return name; } set { name = value; } }
		
		protected JObject jsonData = null;

		protected JsonPreset(ePreset _pType) { pType = _pType; pid = 0; name = ""; }

		public JsonPreset(ePreset _pType, int _pid, string _name)
		{
			pType = _pType;
			pid = _pid;
			name = _name;
		}

		public JsonPreset(JObject data)
		{
			jsonData = data;

			if (data.ContainsKey("ptype"))
			{
				pType = (ePreset)data["ptype"].Value<int>();
			}

			if (data.ContainsKey("name"))
			{
				name = data["name"].Value<string>();
			}

			if (data.ContainsKey("pid"))
			{
				pid = data["pid"].Value<int>();
			}
		}

		override public string ToString()
		{
			return jsonData.ToString();
		}

		virtual public JObject ToJObject()
		{
			if (jsonData == null)
			{
				jsonData = new JObject();
			}
			
			if(pid > 0)
			{
				AddOrOverwrite("ptype", (int)pType);
				AddOrOverwrite("pid", pid);
			}
				
			if(name != "")
			{
				AddOrOverwrite("name", name);
			}			

			return jsonData;
		}

		protected void AddOrOverwrite(string key, JToken value)
		{
			if (!jsonData.ContainsKey(key))
			{
				jsonData.Add(key, value);
			}
			else
			{
				jsonData[key] = value;
			}
		}
	}

	public class PresetDragon: JsonPreset
	{
		int did;
		public int DragonID { get { return did; } /*set*/ }

		int exp;
		public int Exp { get { return exp; } set { exp = value; } }

		int slvl;
		public int SkillLevel { get { return slvl; } set { slvl = value; } }

		PresetPart[] parts;
		public PresetPart[] PartsArray { get { return parts; } set { parts = value; } }

		PresetPet pet;
		public PresetPet Pet { get { return pet; } set { pet = value; } }


		public PresetDragon(int _pid, string _name, int _did, int _exp, int _slvl, PresetPart[] _parts = null, PresetPet _pet = null) : base (ePreset.DRAGON, _pid, _name)
		{
			did = _did;
			exp = _exp;
			slvl = _slvl;

			if (_parts == null)
			{
				parts = new PresetPart[0];
			}
			else
			{
				parts = _parts;
			}

			if (_pet == null)
			{
				pet = new PresetPet();
			}
			else
			{
				pet = _pet;
			}
		}

		public PresetDragon(JObject data) : base(data)
		{
			if (data.ContainsKey("did"))
			{
				did = data["did"].Value<int>();
			}

			if (data.ContainsKey("exp"))
			{
				exp = data["exp"].Value<int>();
			}

			if (data.ContainsKey("slvl"))
			{
				slvl = data["slvl"].Value<int>();
			}

			if (data.ContainsKey("pet"))
			{
				pet = new PresetPet((JObject)data["pet"]);
			}

			if (data.ContainsKey("parts"))
			{
				JArray jarr = (JArray)data["parts"];
				parts = new PresetPart[jarr.Count];

				for (int i = 0; i < jarr.Count; i++)
				{
					parts[i] = new PresetPart((JObject)jarr[i]);
				}
			}
		}

		override public JObject ToJObject()
		{
			jsonData = base.ToJObject();

			AddOrOverwrite("did", did);
			AddOrOverwrite("exp", exp);
			AddOrOverwrite("slvl", slvl);
			AddOrOverwrite("pet", pet.ToJObject());

			if(parts != null && parts.Length > 0)
            {
				JArray partArr = new JArray();
				for(var i = 0; i < parts.Length; i++)
                {
					JObject temp = new JObject();
					var part = parts[i];
					if(part != null)
                    {
						temp = part.ToJObject();
                    }
					partArr.Add(temp);
                }

				AddOrOverwrite("parts", partArr);
			}

			return jsonData;
		}
	}

	public class PresetPart: JsonPreset, ITableData
	{
		int id;
		public int PartID { get { return id; } }

		int lvl;
		public int Level { get { return lvl; } }

		int[] subs;
		public int[] Subs { get { return subs; } }

		public PresetPart() : base(ePreset.PART)
		{
			jsonData = new JObject();
		}

		public PresetPart(JObject data) : base(data)
		{
			if (data.ContainsKey("equip_id"))
			{
				id = data["equip_id"].Value<int>();
			}

			if (data.ContainsKey("level"))
			{
				lvl = data["level"].Value<int>();
			}

			if (data.ContainsKey("subs"))
			{
				JArray jarr = (JArray)data["subs"];
				subs = new int[jarr.Count];

				for (int i = 0; i < jarr.Count; i++)
				{
					subs[i] = jarr[i].Value<int>();
				}
			}
		}
		public PresetPart(int _pid, string _name, int _id, int _lvl, int[] _subs) : base(ePreset.PART, _pid, _name)
		{
			id = _id;
			lvl = _lvl;
			subs = _subs;
		}

		override public JObject ToJObject()
		{
			jsonData = base.ToJObject();
			JArray jarr = new JArray();

			for (int i = 0; i < subs.Length; i++)
			{
				jarr.Add(subs[i]);
			}

			AddOrOverwrite("equip_id", id);
			AddOrOverwrite("level", lvl);
			AddOrOverwrite("subs", jarr);

			return jsonData;
		}

		public void Init() { }
		public string GetKey() { return PartID.ToString(); }
	}

	public class PresetPet: JsonPreset, ITableData
	{
		int id;
		public int PetID { get { return id; } }

		int exp;
		public int Exp { get { return exp; } }

		List<int> passive;
		public List<int> Passives { get { return passive; } }

		public PresetPet() : base(ePreset.PET)
		{
			jsonData = new JObject();
			passive = new List<int>();
		}

		public PresetPet(JObject data) : base(data)
		{
			if (data.ContainsKey("id"))
			{
				id = data["id"].Value<int>();
			}

			if (data.ContainsKey("exp"))
			{
				exp = data["exp"].Value<int>();
			}

			if (data.ContainsKey("passive"))
			{
				JArray jarr = (JArray)data["passive"];
				passive = new List<int>();

				for (int i = 0; i < jarr.Count; i++)
				{
					passive.Add(jarr[i].Value<int>());
				}
			}
		}
		public PresetPet(int _pid, string _name,int _petID, int _exp, int[] _skills) : base(ePreset.PET, _pid, _name)
		{
			id = _petID;
			exp = _exp;
			if(passive == null)
            {
				passive = new List<int>();
            }
			passive.Clear();
			passive = new List<int>(_skills);
		}

		override public JObject ToJObject()
		{
			jsonData = base.ToJObject();

			if(id > 0)
			{
				AddOrOverwrite("id", id);
				AddOrOverwrite("exp", exp);
				AddOrOverwrite("passive", new JArray(passive.ToArray()));
			}

			return jsonData;
		}
		public void Init() { }
		public string GetKey() { return PetID.ToString(); }
	}

	public class SimulatorPreset
	{
		readonly public static string DIRECTORY_DOCUMENT = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
		readonly public static string DIRECTORY_MTD = "MTD_DEV";
		readonly public static string DIRECTORY_MTD_PATH = DIRECTORY_DOCUMENT + "\\" + DIRECTORY_MTD;
		readonly public static string FILE_PRESET = "presets.json";
		readonly private static string KEY_DRAGON = "dragons";
		readonly private static string KEY_PART = "parts";
		readonly private static string KEY_PET = "pets";

		static DirectoryInfo directory = null;
		static FileStream preset = null;
		static JObject presetData = null;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void InitPlayMode()
		{
			if (directory != null)
			{
				directory = null;
			}

			if(preset != null)
			{
				preset.Close();
				preset = null;
			}

			if (presetData != null)
			{
				presetData.RemoveAll();
				presetData = null;
			}
		}

		public static void ReadMyDocument()
		{
			if (directory == null)
			{
				directory = new DirectoryInfo(DIRECTORY_MTD_PATH);

				if (!directory.Exists)
				{
					directory = Directory.CreateDirectory(DIRECTORY_MTD_PATH);
				}
			}

			//using으로 변경하기
			if (preset != null)
			{
				preset.Close();
			}

			using (preset = new FileStream(DIRECTORY_MTD_PATH + "/" + FILE_PRESET, FileMode.OpenOrCreate))
			{
				if (presetData != null)
				{
					presetData.RemoveAll();
				}

				int size = (int)preset.Length;

				if (size == 0)
				{
					CreateDefaultJObject();
					return;
				}

				Byte[] buffer = new Byte[size];
				int read = preset.Read(buffer, 0, size);

				if (read == size)
				{
					string convert = UTF8Encoding.UTF8.GetString(buffer);
					presetData = JObject.Parse(convert);
				}
			}
		}

		public static List<JsonPreset> GetPresetDragons()
		{
			List<JsonPreset> dragons = new();

			if (presetData.ContainsKey(KEY_DRAGON))
			{
				JArray jarr = (JArray)presetData[KEY_DRAGON];

				for (int i = 0; i < jarr.Count; i++)
				{
					dragons.Add(new PresetDragon((JObject)jarr[i]));
				}
			}

			return dragons;
		}

		public static List<JsonPreset> GetPresetParts()
		{
			List<JsonPreset> parts = new();

			if (presetData.ContainsKey(KEY_PART))
			{
				JArray jarr = (JArray)presetData[KEY_PART];

				for (int i = 0; i < jarr.Count; i++)
				{
					parts.Add(new PresetPart((JObject)jarr[i]));
				}
			}

			return parts;
		}
		public static List<JsonPreset> GetPresetPets()
		{
			List<JsonPreset> pets = new();

			if (presetData.ContainsKey(KEY_PET))
			{
				JArray jarr = (JArray)presetData[KEY_PET];

				for (int i = 0; i < jarr.Count; i++)
				{
					pets.Add(new PresetPet((JObject)jarr[i]));
				}
			}

			return pets;
		}

		public static void ApplyPreset(JsonPreset data)
		{
			string presetName = FILE_PRESET;
			bool edited = false;

			switch (data.PresetType)
			{
				case ePreset.DRAGON:
				{
					JArray Dragons = null;
					if(presetData.ContainsKey(KEY_DRAGON))
					{
						Dragons = ((JArray)presetData[KEY_DRAGON]);
					}
					else
					{
						Dragons = new JArray();
						presetData.Add(KEY_DRAGON, Dragons);
					}

					for (int i = 0; i < Dragons.Count; i++)
					{
						if(new PresetDragon((JObject)Dragons[i]).PresetID == data.PresetID)
						{
							Dragons[i] = data.ToJObject();
							edited = true;
							break;
						}
					}

					if (!edited)
					{
						Dragons.Add(data.ToJObject());
						break;
					}
				}
				break;
				case ePreset.PART:
				{
					JArray Parts = null;

					if (presetData.ContainsKey(KEY_PART))
					{
						Parts = ((JArray)presetData[KEY_PART]);
					}
					else
					{
						Parts = new JArray();
						presetData.Add(KEY_PART, Parts);
					}

					for (int i = 0; i < Parts.Count; i++)
					{
						if (new PresetPart((JObject)Parts[i]).PresetID == data.PresetID)
						{
							Parts[i] = data.ToJObject();
							edited = true;
							break;
						}
					}

					if (!edited)
					{
						Parts.Add(data.ToJObject());
						break;
					}
				}
				break;
				case ePreset.PET:
				{
					JArray Pets = (JArray)(presetData[KEY_PET]);

					if (presetData.ContainsKey(KEY_PET))
					{
						Pets = ((JArray)presetData[KEY_PET]);
					}
					else
					{
						Pets = new JArray();
						presetData.Add(KEY_PET, Pets);
					}

					for (int i = 0; i < Pets.Count; i++)
					{
						if (new PresetPet((JObject)Pets[i]).PresetID == data.PresetID)
						{
							Pets[i] = data.ToJObject();
							edited = true;
							break;
						}
					}

					if (!edited)
					{
						Pets.Add(data.ToJObject());
						break;
					}
				}
				break;
			}
		}

		public static void SavePreset()
		{
			string serlialize = presetData.ToString();
			//JsonConvert.SerializeObject(presetData);
			char[] charstream = serlialize.ToString().ToCharArray();
			byte[] dataStream = new UTF8Encoding(true).GetBytes(charstream, 0, charstream.Length);

			using (preset = new FileStream(DIRECTORY_MTD_PATH + "/" + FILE_PRESET, FileMode.Create))
			{
				if (preset.CanWrite)
				{
					preset.Write(dataStream, 0, dataStream.Length);
					preset.Close();
				}
			}
		}

		public static int GetPresetIDMax(ePreset pType)
		{
			int max = 0;

			List<JsonPreset> data = null;
			if (presetData == null)
			{
				ReadMyDocument();
			}

			switch (pType)
			{
				case ePreset.DRAGON:
					data = GetPresetDragons();
					break;
				case ePreset.PART:
					data = GetPresetParts();
					break;
				case ePreset.PET:
					data = GetPresetPets();
					break;
				default:
					break;
			}

			if(data != null)
			{
				for (int i = 0; i < data.Count; i++)
				{
					max = Mathf.Max(max, data[i].PresetID);
				}
			}

			return max;
		}
		private static void CreateDefaultJObject()
		{
			if(presetData != null)
			{
				return;
			}

			presetData = new JObject();
			presetData.Add(KEY_PART, new JArray());
			presetData.Add(KEY_PET, new JArray());
			presetData.Add(KEY_DRAGON, new JArray());
		}

		public static bool isDuplicationPresetName(string _presetName, ePreset pType)
        {
			List<JsonPreset> data = null;

			switch (pType)
			{
				case ePreset.DRAGON:
					data = GetPresetDragons();
					break;
				case ePreset.PART:
					data = GetPresetParts();
					break;
				case ePreset.PET:
					data = GetPresetPets();
					break;
				default:
					break;
			}

			if(data == null || data.Count <= 0)
            {
				return false;
            }

			for(var i = 0; i < data.Count; i++)
            {
				var preset = data[i];
				if(preset == null)
                {
					continue;
                }
				var name = preset.PresetName;
				if(name == _presetName)
                {
					return true;
                }
            }
			return false;
		}
		public static int GetPresetID(string _presetName, ePreset pType)
		{
			List<JsonPreset> data = null;

			switch (pType)
			{
				case ePreset.DRAGON:
					data = GetPresetDragons();
					break;
				case ePreset.PART:
					data = GetPresetParts();
					break;
				case ePreset.PET:
					data = GetPresetPets();
					break;
				default:
					break;
			}

			if (data == null || data.Count <= 0)
			{
				return -1;
			}

			for (var i = 0; i < data.Count; i++)
			{
				var preset = data[i];
				if (preset == null)
				{
					continue;
				}
				var name = preset.PresetName;
				if (name == _presetName)
				{
					return preset.PresetID;
				}
			}
			return -1;
		}
	}
}

#endif