﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EdgeGuidance : MonoBehaviour
{
    public static EdgeGuidance instance;
    public float lerpSpeed = 6.0f;
    public GameObject defaultIcon;

    //List of all targets.
    List<UITarget> targets = new List<UITarget>();
    Camera mainCamera;
    Vector2 CamBound;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        mainCamera = transform.parent.GetComponent<Camera>();
        CamBound = new Vector2(mainCamera.pixelWidth, mainCamera.pixelHeight) / 2;
    }
}
    /*
    // Update is called once per frame
    void Update()
    {
        UpdateGuide();
    }
    //Update as it uses centre of grid atm
    bool CheckOutOfView(UITarget target)
    {
        Vector3 screenPoint = mainCamera.WorldToViewportPoint(target.targetTransform.position);
        bool OutOfView = !(screenPoint.z > 0 && screenPoint.x > 0
            && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1);

        target.guideTransform.gameObject.SetActive(OutOfView);

        return OutOfView;
    }

    public void AddTarget(Transform target, Sprite icon, Color colour)
    {
        GameObject targetGuide = new GameObject("guide" + targets.Count);

        targetGuide.transform.parent = transform;
        Image img = targetGuide.AddComponent<Image>();
        img.sprite = icon;
        img.color = colour;
        img.rectTransform.position = Vector3.zero;

        targets.Add(new UITarget(target, targetGuide.GetComponent<Image>().rectTransform));
    }

    public void AddTarget(Transform target,  Color colour)
    {
        //Instantiate the circle
        GameObject targetGuide = Instantiate(defaultIcon, transform);
        
        //centre the circle on player
        Vector3 guidePos = mainCamera.transform.position;
        guidePos.y = target.position.y;
        targetGuide.transform.position = guidePos;

        //Set Scale
        float scale = Vector3.Distance(guidePos, target.position);
        targetGuide.transform.localScale += new Vector3(scale, 0, scale); // set X and Z scale to 0 for prefab!!

        //targets.Add(new UITarget(target, targetGuide.GetComponent<Image>().rectTransform));

        targetGuide.SetActive(true);
    }

    public void RemoveTarget(Transform pos)
    {
        foreach (UITarget target in targets)
        {
            if (target.targetTransform = pos)
            {
                targets.Remove(target);
                return;
            }
        }
    }

    void UpdateGuide()
    {
        foreach (UITarget target in targets)
        {
            if (CheckOutOfView(target))
            {
            }
            Vector3 targetDir = target.targetTransform.position - mainCamera.transform.position;
            float angle = Vector3.SignedAngle(targetDir, mainCamera.transform.forward, Vector3.up) * Mathf.PI / 180;
            float angleY = Vector3.SignedAngle(targetDir, mainCamera.transform.right, Vector3.up) * Mathf.PI / 180;


            if (Mathf.Abs(angle) > (Mathf.PI / 2))
            {
                if (angle > 0)
                {
                    angle = Mathf.PI - angle;
                }
                else
                {
                    angle = -Mathf.PI - angle;
                }
            }
            if (Mathf.Abs(angleY) > (Mathf.PI / 2))
            {
                if (angleY > 0)
                {
                    angleY = Mathf.PI - angleY;
                }
                else
                {
                    angleY = -Mathf.PI - angleY;
                }
            }

            float xPos = Mathf.Clamp(-Mathf.Tan(angle) * CamBound.y, -CamBound.x, CamBound.x);
            float yPos = Mathf.Clamp(Mathf.Tan(angleY) * CamBound.x, -CamBound.y, CamBound.y);

            //Could possibly change, fixes jump at 180degrees
            target.guideTransform.localPosition = Vector3.Lerp(target.guideTransform.localPosition,
                new Vector3(xPos, yPos, 0), Time.deltaTime * lerpSpeed);

            target.guideTransform.localRotation = Quaternion.Euler(0, 0, Vector3.SignedAngle(targetDir, mainCamera.transform.forward, Vector3.up));
        }
    }

}
*/
