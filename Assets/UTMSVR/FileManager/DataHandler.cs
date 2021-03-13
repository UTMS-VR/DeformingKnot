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

    public class CurveCore
    {
        public List<Vector3> points;
        public bool closed;

        public CurveCore(List<Vector3> points, bool closed)
        {
            this.points = points;
            this.closed = closed;
        }
    }

    [Serializable]
    public class SerializedCurveCore
    {
        [SerializeField]
        public List<Vector3> points;
        [SerializeField]
        public bool closed;

        public SerializedCurveCore(List<Vector3> points, bool closed)
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

        public void SaveCurves(string filename, List<(List<Vector3> points, bool closed)> curves)
        {
            List<CurveCore> curveCores = curves.Select(curve => new CurveCore(curve.points, curve.closed)).ToList();
            List<CurveCore> normalizedCurves = DataHandler.Normalize(curveCores);
            List<SerializedCurveCore> serializedCurves = normalizedCurves.Select(curve => new SerializedCurveCore(curve.points, curve.closed)).ToList();
            string json = JsonUtility.ToJson(new SerializedList<SerializedCurveCore>(serializedCurves));
            this.Save(filename, json);
        }

        public void SaveCurve(string filename, List<Vector3> points, bool closed)
        {
            this.SaveCurves(filename, new List<(List<Vector3>, bool)>{ (points, closed) });
        }

        public List<(List<Vector3> points, bool closed)> LoadCurves(string filename, float maxLength = 1.0f, Vector3? barycenter = null)
        {
            string json = this.Load(filename);
            List<SerializedCurveCore> serializedCurves = JsonUtility.FromJson<SerializedList<SerializedCurveCore>>(json).ToList();
            List<CurveCore> curves = serializedCurves.Select(curve => new CurveCore(curve.points, curve.closed)).ToList();
            List<CurveCore> adjustedCurves = DataHandler.Normalize(curves, maxLength, barycenter);
            return adjustedCurves.Select(curve => (curve.points, curve.closed)).ToList();
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

        public List<(List<Vector3> points, bool closed)> LoadCurvesFromGitHub(
            string remoteFilename, string localFilename = null, float maxLength = 1.0f, Vector3? barycenter = null
            )
        {
            string url = $"https://raw.githubusercontent.com/UTMS-VR/CurveData/main/{remoteFilename}";
            string json = this.LoadStringFromUrl(url, localFilename ?? remoteFilename);
            List<SerializedCurveCore> serializedCurves = JsonUtility.FromJson<SerializedList<SerializedCurveCore>>(json).ToList();
            List<CurveCore> curves = serializedCurves.Select(curve => new CurveCore(curve.points, curve.closed)).ToList();
            List<CurveCore> adjustedCurves = DataHandler.Normalize(curves, maxLength, barycenter);
            return adjustedCurves.Select(curve => (curve.points, curve.closed)).ToList();
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

        private static List<Vector3> ConcatAllCurves(List<CurveCore> curves)
        {
            List<List<Vector3>> pointsList = curves.Select(curve => curve.points).ToList();
            return pointsList.Aggregate((points1, points2) => points1.Concat(points2).ToList()).ToList();   
        }

        private static List<CurveCore> MoveBarycenter(List<CurveCore> curves, Vector3? barycenter = null)
        {
            List<Vector3> allPoints = DataHandler.ConcatAllCurves(curves);
            if (allPoints.Count == 0)
            {
                return curves;
            }
            Vector3 barycenterOfCurves = allPoints.Aggregate((v, w) => v + w) / allPoints.Count;
            Vector3 newBarycenter = barycenter ?? Vector3.zero;
            return curves.Select(
                curve => new CurveCore(curve.points.Select(v => v - barycenterOfCurves + newBarycenter).ToList(), curve.closed)
            ).ToList();
        }

        static List<CurveCore> Normalize(List<CurveCore> curves, float maxLength = 1.0f, Vector3? barycenter = null)
        {
            List<CurveCore> movedCurves = DataHandler.MoveBarycenter(curves);
            List<Vector3> allPoints = DataHandler.ConcatAllCurves(movedCurves);
            float maxLengthInCurves = 0.0f;
            foreach (Vector3 vector in allPoints)
            {
                if (vector.magnitude > maxLengthInCurves)
                {
                    maxLengthInCurves = vector.magnitude;
                }
            }
            if (maxLengthInCurves == 0.0f)
            {
                return DataHandler.MoveBarycenter(movedCurves, barycenter);
            }
            List<CurveCore> normalizedCurves = movedCurves.Select(
                curve => new CurveCore(curve.points.Select(v => v * maxLength / maxLengthInCurves).ToList(), curve.closed)
            ).ToList();
            return DataHandler.MoveBarycenter(normalizedCurves, barycenter);
        }

        public List<string> GetFiles()
        {
            string inputDir = this.onHMD() ? this.inputDirOnHMD : this.inputDirOnPC;
            Directory.CreateDirectory(inputDir);
            List<string> filenames = Directory.GetFiles(inputDir, "*.json", SearchOption.TopDirectoryOnly).ToList();
            return filenames.Select(name => name.Substring(inputDir.Length + 1)).ToList();
        }
    }
}

