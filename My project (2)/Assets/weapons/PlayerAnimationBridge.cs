using UnityEngine;

public class PlayerAnimationBridge : MonoBehaviour
{
    // Referensi ke script kelenturan sarung
    private SarungFlex sarungFlexScript;

    void Start()
    {
        // Mencari script di anak-anak karakter (karena sarung menempel di tangan)
        sarungFlexScript = GetComponentInChildren<SarungFlex>();

        if (sarungFlexScript == null)
        {
            Debug.LogError("Gagal menemukan script SarungFlex di anak karakter! Pastikan sarung sudah menempel di tangan.");
        }
    }

    // Fungsi ini yang akan muncul di dropdown Animation Event
    public void CallSarungSnap()
    {
        if (sarungFlexScript != null)
        {
            // Memanggil fungsi asli di senjata
            sarungFlexScript.TriggerSnap(); 
        }
    }
}