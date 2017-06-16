using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trash : MonoBehaviour
{

    private float fadeDelay = 0.0f;
    private float fadeTime = 0.5f;
    private bool fadeInOnStart = false;
    private bool fadeOutOnStart = false;
    private bool logInitialFadeSequence = false;
    // store colours
    private Color[] colors;


    private Vector2 m_smashObjectStartPosition;

    
    private Rigidbody2D[] m_trashPartRigs;
    private Vector2[] m_trashPartOriginalVectors;
    private SpriteRenderer[] m_trashPartRenderers;
    private bool m_isSmashing;

    private float m_alpha;

    private Rigidbody2D m_rigidBody;

    private GameObject m_smashEffectObj;

    // Use this for initialization
    void Start()
    {
        m_rigidBody = GetComponent<Rigidbody2D>();

        m_alpha = 1f;
        m_isSmashing = false;
        m_smashEffectObj = GameObject.Find("/lv01-01/conveyor-trash/SmashTrashEffect");
        m_trashPartRigs = m_smashEffectObj.GetComponentsInChildren<Rigidbody2D>();
        m_trashPartRenderers = m_smashEffectObj.GetComponentsInChildren<SpriteRenderer>();

        m_trashPartOriginalVectors = new Vector2[m_trashPartRigs.Length];
        m_smashObjectStartPosition = m_smashEffectObj.transform.position;
        for (int i = 0; i < m_trashPartOriginalVectors.Length; i++)
        {
            m_trashPartOriginalVectors[i] = m_trashPartRigs[i].position;
        }
    }
	
	// Update is called once per frame
	void Update()
    {
        //Debug.Log(m_isSmashing);

        UpdateTrashParts();
	}

    void OnCollisionEnter2D(Collision2D col)
    {
        if (Mathf.Abs(col.relativeVelocity.y) > 10)
        {
            // track that we are now smashing
            m_isSmashing = true;

            // disable object's sprite renderer and collider components
            gameObject.GetComponent<SpriteRenderer>().enabled = false;
            gameObject.GetComponent<Collider2D>().enabled = false;

            // set smash effect's position and activate
       
            m_smashEffectObj.transform.position = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y);

            //Debug.Log(m_smashEffectObj.transform.position);

            //foreach (Rigidbody2D rig in m_trashPartRigs)
            //{
            //    Debug.Log(rig.position);
            //}

            m_smashEffectObj.SetActive(true);

            // add force to trash parts' rigid bodies
            foreach (Rigidbody2D rig in m_trashPartRigs)
            {
                rig.velocity = Vector2.zero;
                rig.AddForce(new Vector2(Random.Range(-6, 6), Mathf.Abs(col.relativeVelocity.y / 2)), ForceMode2D.Impulse);       
            }           
        }
    }

    private void UpdateTrashParts()
    {
        // check that object has started smashing
        if (m_isSmashing)
        {
            // don't do anything unless all the trash parts have stopped moving
            foreach (Rigidbody2D rig in m_trashPartRigs)
            {
                //if (rig.velocity != Vector2.zero)
                //    return;
            }

            

            foreach (SpriteRenderer renderer in m_trashPartRenderers)
            {
                // update color's alpha channel
                Color color = renderer.material.color;
                color.a = m_alpha;
                renderer.material.SetColor("_Color", color);             
            }

            if (m_alpha >= 0)
            {
                // decrement alpha
                m_alpha -= 0.02f;
            } else
            {
                //m_smashEffectObj.transform.position = m_smashObjectStartPosition;

                for (int i = 0; i < m_trashPartOriginalVectors.Length; i++)
                {
                    m_trashPartRigs[i].position = new Vector2(0, 0);
                }


                foreach (SpriteRenderer renderer in m_trashPartRenderers)
                {
                    // update color's alpha channel
                    Color color = renderer.material.color;
                    color.a = 1f;
                    renderer.material.SetColor("_Color", color);
                }

                m_smashEffectObj.SetActive(false);

                m_alpha = 1f;

                m_isSmashing = false;
            }
        }


        //reduce opacity










        // if opacity <= 0 then f

    }

    private void ResetTrashParts()
    {
        foreach (Rigidbody2D rig in m_trashPartRigs)
        {
            if (rig.velocity != Vector2.zero)
                continue;

            SpriteRenderer renderer = rig.GetComponentInParent<SpriteRenderer>();
            
            rig.position = m_smashObjectStartPosition;
        }
    }
    





    // check the alpha value of most opaque object
    float MaxAlpha()
    {
        float maxAlpha = 0.0f;
        Renderer[] rendererObjects = GetComponentsInChildren<Renderer>();
        foreach (Renderer item in rendererObjects)
        {
            maxAlpha = Mathf.Max(maxAlpha, item.material.color.a);
        }
        return maxAlpha;
    }

    // fade sequence
    IEnumerator FadeSequence(float fadingOutTime)
    {
        // log fading direction, then precalculate fading speed as a multiplier
        bool fadingOut = (fadingOutTime < 0.0f);
        float fadingOutSpeed = 1.0f / fadingOutTime;

        // grab all child objects
        Renderer[] rendererObjects = GetComponentsInChildren<Renderer>();
        if (colors == null)
        {
            //create a cache of colors if necessary
            colors = new Color[rendererObjects.Length];

            // store the original colours for all child objects
            for (int i = 0; i < rendererObjects.Length; i++)
            {
                colors[i] = rendererObjects[i].material.color;
            }
        }

        // make all objects visible
        for (int i = 0; i < rendererObjects.Length; i++)
        {
            rendererObjects[i].enabled = true;
        }


        // get current max alpha
        float alphaValue = MaxAlpha();


        // This is a special case for objects that are set to fade in on start. 
        // it will treat them as alpha 0, despite them not being so. 
        //if (logInitialFadeSequence && !fadingOut)
        //{
        //    alphaValue = 0.0f;
        //    logInitialFadeSequence = false;
        //}

        // iterate to change alpha value 
        while ((alphaValue >= 0.0f && fadingOut) || (alphaValue <= 1.0f && !fadingOut))
        {
            alphaValue += Time.deltaTime * fadingOutSpeed;

            for (int i = 0; i < rendererObjects.Length; i++)
            {
                Color newColor = (colors != null ? colors[i] : rendererObjects[i].material.color);
                newColor.a = Mathf.Min(newColor.a, alphaValue);
                newColor.a = Mathf.Clamp(newColor.a, 0.0f, 1.0f);
                rendererObjects[i].material.SetColor("_Color", newColor);
            }

            yield return null;
        }

        // turn objects off after fading out
        if (fadingOut)
        {
            for (int i = 0; i < rendererObjects.Length; i++)
            {
                rendererObjects[i].enabled = false;
            }
        }


       // Debug.Log("fade sequence end : " + fadingOut);

    }


    void FadeIn()
    {
        FadeIn(fadeTime);
    }

    void FadeOut()
    {
        FadeOut(fadeTime);
    }

    void FadeIn(float newFadeTime)
    {
        StopAllCoroutines();
        StartCoroutine("FadeSequence", newFadeTime);
    }

    void FadeOut(float newFadeTime)
    {
        StopAllCoroutines();
        StartCoroutine("FadeSequence", -newFadeTime);
    }
}
