using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;

namespace FileManager
{
    [Serializable]
    public class SerializedList<T>
    {
        // https://kou-yeung.hatenablog.com/entry/2015/12/31/014611
        [SerializeField]
        List<T> target;
        public List<T> ToList() { return this.target; }
        public SerializedList(List<T> target)
        {
            this.target = target;
        }
    }

    [Serializable]
    public class CurveCore
    {
        [SerializeField]
        public List<Vector3> points;
        [SerializeField]
        public bool closed;

        public CurveCore(List<Vector3> points, bool closed)
        {
            this.points = points;
            this.closed = closed;
        }
    }

    public class DataHandler
    {
        string inputDirOnPC;
        string outputDirOnPC;
        string inputDirOnHMD;
        string outputDirOnHMD;
        string cacheDirOnPC;
        string cacheDirOnHMD;

        public DataHandler(
            string inputDirOnPC = "input", string outputDirOnPC = null,
            string inputDirOnHMD = null, string outputDirOnHMD = "/mnt/sdcard/output",
            string cacheDirOnPC = "cacheFromFileManager", string cacheDirOnHMD = "/mnt/sdcard/cacheFromFileManager"
            )
        {
            this.inputDirOnPC = inputDirOnPC;
            this.outputDirOnPC = outputDirOnPC;
            this.inputDirOnHMD = inputDirOnHMD;
            this.outputDirOnHMD = outputDirOnHMD;
            this.cacheDirOnPC = cacheDirOnPC;
            this.cacheDirOnHMD = cacheDirOnHMD;
        }

        public void Save(string filename, string text)
        {
            string outputDir = this.onHMD() ? this.outputDirOnHMD : this.outputDirOnPC;
            if (outputDir == null)
            {
                Debug.Log("Cannot write file since outputDir is null");
                return;
            }
            Directory.CreateDirectory(outputDir);
            using (var writer = new StreamWriter(Path.Combine(outputDir, filename)))
            {
                writer.Write(text);
            }
        }

        public string Load(string filename)
        {
            string inputDir = this.onHMD() ? this.inputDirOnHMD : this.inputDirOnPC;
            if (inputDir == null)
            {
                throw new Exception("Cannot load file since inputDir is null");
            }
            using (var reader = new StreamReader(Path.Combine(inputDir, filename)))
            {
                return reader.ReadToEnd();
            }
        }

        public void SaveCurve(string filename, List<Vector3> curve)
        {
            List<Vector3> normalizedCurve = DataHandler.Normalize(curve);
            string json = JsonUtility.ToJson(new SerializedList<Vector3>(normalizedCurve));
            this.Save(filename, json);
        }

        public void SaveCurves(string filename, List<(List<Vector3> points, bool closed)> curves)
        {
            List<CurveCore> serializedCurves = curves.Select(curve => new CurveCore(curve.points, curve.closed)).ToList();
            string json = JsonUtility.ToJson(new SerializedList<CurveCore>(serializedCurves));
            this.Save(filename, json);
        }

        public List<Vector3> LoadCurve(string filename, float maxLength = 1.0f, Vector3? barycenter = null)
        {
            string json = this.Load(filename);
            List<Vector3> curve = JsonUtility.FromJson<SerializedList<Vector3>>(json).ToList();
            return DataHandler.Normalize(curve, maxLength, barycenter);
        }

        public List<(List<Vector3> points, bool closed)> LoadCurves(string filename)
        {
            string json = this.Load(filename);
            List<CurveCore> serializedCurves = JsonUtility.FromJson<SerializedList<CurveCore>>(json).ToList();
            return serializedCurves.Select(curve => (curve.points, curve.closed)).ToList();
        }

        public string LoadStringFromUrl(string url, string filename)
        {
            string cacheDir = this.onHMD() ? this.cacheDirOnHMD : this.cacheDirOnPC;
            string filepath = Path.Combine(cacheDir, filename);
            if (!File.Exists(filepath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filepath));
                using (var client = new System.Net.WebClient())
                {
                    Debug.Log(url);
                    client.DownloadFile(url, filepath);
                }
            }
            using (var reader = new StreamReader(filepath))
            {
                return reader.ReadToEnd();
            }
        }

        public List<Vector3> LoadCurveFromGitHub(
            string remoteFilename, string localFilename = null, float maxLength = 1.0f, Vector3? barycenter = null
            )
        {
            string url = $"https://raw.githubusercontent.com/UTMS-VR/CurveData/main/{remoteFilename}";
            string json = this.LoadStringFromUrl(url, localFilename ?? remoteFilename);
            List<Vector3> curve = JsonUtility.FromJson<SerializedList<Vector3>>(json).ToList();
            return DataHandler.Normalize(curve, maxLength, barycenter);
        }

        private bool onHMD()
        {
            // string productName = OVRPlugin.productName;
            // return !(productName == null || productName == "");
            // ↑うまくいかなかった(前はうまくいってた気がするけど、何故？)
#if UNITY_EDITOR
            return false;
#else
            return true;
#endif            
        }

        private static List<Vector3> MoveBarycenter(List<Vector3> curve, Vector3? barycenter = null)
        {
            if (curve.Count == 0)
            {
                return curve;
            }
            Vector3 barycenterOfCurve = curve.Aggregate((v, w) => v + w) / curve.Count;
            Vector3 newBarycenter = barycenter ?? Vector3.zero;
            return curve.Select(v => v - barycenterOfCurve + newBarycenter).ToList();
        }

        static List<Vector3> Normalize(List<Vector3> curve, float maxLength = 1.0f, Vector3? barycenter = null)
        {
            List<Vector3> movedCurve = DataHandler.MoveBarycenter(curve);
            float maxLengthInCurve = 0.0f;
            foreach (Vector3 vector in movedCurve)
            {
                if (vector.magnitude > maxLengthInCurve)
                {
                    maxLengthInCurve = vector.magnitude;
                }
            }
            List<Vector3> normalizedCurve = movedCurve.Select(
                vector => (vector.magnitude == 0.0f) ? vector : vector * maxLength / maxLengthInCurve
                ).ToList();
            return DataHandler.MoveBarycenter(normalizedCurve, barycenter);
        }

        public List<string> GetFilenames()
        {
            string inputDir = this.onHMD() ? this.inputDirOnHMD : this.inputDirOnPC;
            Directory.CreateDirectory(inputDir);
            List<string> filenames = Directory.GetFiles(inputDir, "*.json", SearchOption.TopDirectoryOnly).ToList();
            return filenames.Select(name => name.Substring(inputDir.Length + 1)).ToList();
        }
    }
}

