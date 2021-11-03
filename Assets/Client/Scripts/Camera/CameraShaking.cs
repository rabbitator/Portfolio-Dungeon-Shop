using DungeonShop.GameLogic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraShaking : MonoBehaviour
{
#pragma warning disable 0649

#pragma warning restore 0649

    private float duration = 0.2f;
    private float amplitude = 0.05f;
    private float speed = 50.0f;
    private float amplitudeMultiplier = 3.0f;
    private GameConfiguration gameConfiguration;
    private static CameraShaking instance;
    private Transform thisTransform;
    private float remainOffset;

    private void Awake()
    {
        thisTransform = transform;
        Camera camera = GetComponent<Camera>();
        if(camera != null && Camera.main == camera)
        {
            instance = this;
        }
    }

    private void Start()
    {
        gameConfiguration = ConfigurationObject.GameConfiguration;
        if (gameConfiguration != null)
        {
            ApplySettings();
        }
        else
        {
            Debug.LogError($"{name} (CameraShaking): Can't get Game Configuration!");
        }
    }

    public static void ShakeCamera(bool multiplied)
    {
        if (instance == null)
            return;

        instance.StopAllCoroutines();
        instance.thisTransform.position -= Vector3.right * instance.remainOffset;
        instance.remainOffset = 0.0f;

        instance.StartCoroutine(instance.Shaking(multiplied));
    }

    private IEnumerator Shaking(bool multiplied)
    {
        float startTime = Time.time;
        while (Time.time - startTime < duration)
        {
            float multiplier = multiplied ? amplitudeMultiplier : 1.0f;
            float deltaOffset = amplitude * multiplier * Mathf.Sin(Time.time * speed);
            remainOffset += deltaOffset;
            thisTransform.position += Vector3.right * deltaOffset;

            yield return null;
        }

        thisTransform.position -= Vector3.right * remainOffset;
        remainOffset = 0.0f;
    }

    private void ApplySettings()
    {
        if(gameConfiguration != null)
        {
            duration = gameConfiguration.hitCameraShakingDuration;
            amplitude = gameConfiguration.hitCameraAmplitude;
            speed = gameConfiguration.hitCameraShakingSpeed;
            amplitudeMultiplier = gameConfiguration.hitCameraCriticalAmplitudeMultiplier;
        }
    }
}
