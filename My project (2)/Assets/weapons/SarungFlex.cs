using UnityEngine;
using System.Collections.Generic;

public class SarungFlex : MonoBehaviour
{
    [Header("Bone Setup")]
    public List<Transform> sarungBones; 
    
    [Header("Flex Settings")]
    public float elasticity = 10f;      
    public float drag = 5f;            
    public float maxBend = 30f;        

    [Header("Snap Effect")]
    public float snapForce = 50f;       
    public float snapDuration = 0.1f;  

    [Header("Idle Sway Settings")]
    public float swaySpeed = 2f;       
    public float swayAmount = 1.5f;    

    private List<Quaternion> localRotations;
    private Vector3 lastPosition;
    private bool isSnapping = false;
    private float snapTimer = 0f;

    void Start()
    {
        localRotations = new List<Quaternion>();
        foreach (var bone in sarungBones)
        {
            localRotations.Add(bone.localRotation);
        }
        lastPosition = transform.position;
    }

    public void TriggerSnap()
    {
        isSnapping = true;
        snapTimer = snapDuration;
    }

    void LateUpdate()
    {
        Vector3 velocity = (transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;

        // Mengubah velocity menjadi kecepatan relatif dalam koordinat lokal tangan
        Vector3 relativeVelocity = transform.InverseTransformDirection(velocity);

        float time = Time.time;

        for (int i = 0; i < sarungBones.Count; i++)
        {
            float bendFactor = (i + 1) * drag;
            
            // KUNCI PERBAIKAN: Kita menggunakan tanda NEGATIF (-) pada velocity 
            // untuk menciptakan efek INERSIA/HAMBATAN yang 'tertinggal' di belakang gerakan tangan.
            Quaternion dynamicRotation = Quaternion.Euler(
                Mathf.Clamp(-relativeVelocity.z * bendFactor, -maxBend, maxBend), // Move Forward (+relVel.z) -> Bend Back (negative X rot)
                0,
                Mathf.Clamp(relativeVelocity.x * bendFactor, -maxBend, maxBend)   // Move Right (+relVel.x) -> Bend Left (positive Z rot)
            );

            // Logika ayunan natural (Sway) tetap dipertahankan
            float idleX = Mathf.Sin(time * swaySpeed) * swayAmount * (i + 1);
            float idleZ = Mathf.Cos(time * (swaySpeed * 0.8f)) * swayAmount * (i + 1) * 0.5f;
            Quaternion idleRotation = Quaternion.Euler(idleX, 0, idleZ);

            // Gabungkan rotasi dasar + efek inersia + efek idle
            Quaternion targetRotation = localRotations[i] * dynamicRotation * idleRotation;

            // Logika Snap
            if (isSnapping)
            {
                targetRotation *= Quaternion.Euler(snapForce * bendFactor, 0, 0);
            }

            // Pergerakan halus kembali ke posisi target
            sarungBones[i].localRotation = Quaternion.Slerp(
                sarungBones[i].localRotation, 
                targetRotation, 
                Time.deltaTime * elasticity
            );
        }

        if (isSnapping)
        {
            snapTimer -= Time.deltaTime;
            if (snapTimer <= 0f)
            {
                isSnapping = false;
            }
        }
    }
}