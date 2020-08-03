using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DebugUtil
{
    public class CameraManager // : MonoBehaviour
    {
        private GameObject camera;
        private Stick2DMap move;
        private Stick2DMap rotate;
        private float moveSpeed;
        private float rotateSpeed;
        private float theta;
        private float phi;

        public CameraManager(
            Vector3 initialPosition,
            float initialTheta = 0,
            float initialPhi = 90,
            Stick2DMap move = null,
            Stick2DMap rotate = null,
            float moveSpeed = 0.01f,
            float rotateSpeed = 1.0f
            )
        {
            this.camera = GameObject.Find("OVRCameraRig");
            if (this.camera == null)
            {
                Debug.LogError("GameObject 'OVRCameraRig' が見つかりません。Hierarchyに追加してください。");
            }
            this.camera.transform.position = initialPosition;
            this.camera.transform.forward = this.GetDirection();
            this.theta = initialTheta;
            this.phi = initialPhi;
            this.move = move ?? Stick2DMap.Empty;
            this.rotate = rotate ?? Stick2DMap.Empty;
            this.moveSpeed = moveSpeed;
            this.rotateSpeed = rotateSpeed;
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        public void Update()
        {
            this.UpdatePosition();
            this.UpdateDirection();
        }

        private void UpdatePosition()
        {
            Vector2 input = this.move.ToVector2();
            float c = Mathf.Cos(this.theta * Mathf.PI / 180.0f);
            float s = Mathf.Sin(this.theta * Mathf.PI / 180.0f);
            float x = input.y * c + input.x * s;
            float z = input.y * s - input.x * c;
            this.camera.transform.position += new Vector3(x, 0, z) * this.moveSpeed;
        }

        private void UpdateDirection()
        {
            Vector2 input = this.rotate.ToVector2();
            // 0 <= theta <= 360 じゃなくても良い
            float theta = this.theta + input.x * this.rotateSpeed;
            //theta = Mathf.Max(0, theta);
            //theta = Mathf.Min(360, theta);
            this.theta = theta;
            // 0 <= phi <= 180
            // 真上・真下に向けるとカメラがおかしくなるので、0.1と179.9までにする
            float phi = this.phi - input.y * this.rotateSpeed;
            phi = Mathf.Max(0.1f, phi);
            phi = Mathf.Min(179.9f, phi);
            this.phi = phi;

            this.camera.transform.forward = this.GetDirection();
        }

        public Vector3 GetDirection()
        {
            float thetaRad = this.theta * Mathf.PI / 180.0f;
            float phiRad = this.phi * Mathf.PI / 180.0f;
            float x = Mathf.Cos(thetaRad) * Mathf.Sin(phiRad);
            float y = Mathf.Cos(phiRad);
            float z = Mathf.Sin(thetaRad) * Mathf.Sin(phiRad);
            return new Vector3(x, y, z);
        }
    }
}
