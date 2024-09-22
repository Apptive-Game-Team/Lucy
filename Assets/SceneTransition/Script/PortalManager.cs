using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalManager : SingletonObject<PortalManager>
{
    [SerializeField] SceneDataList sceneDataList;
    [SerializeField] PortalDataList portalDataList;

    private const float CALL_LISTENER_DELAY = 0.1f;

    private List<ISceneChangeListener> listeners = new List<ISceneChangeListener>();

    private void Start()
    {
        DontDestroyOnLoad(this);
        StartCoroutine(CallOnSceneChange());
    }

    public void SetSceneChangeListener(ISceneChangeListener listener)
    {
        listeners.Add(listener);
    }

    private IEnumerator CallOnSceneChange()
    {
        yield return new WaitForSeconds(CALL_LISTENER_DELAY);
        foreach (ISceneChangeListener listener in listeners)
        {
            listener.OnSceneChange();
        }
    }

    public IEnumerator TransitScene(PortalID portalID)
    {
        yield return CameraEffector.Instance.FadeOut();
        PortalData portalData = portalDataList.GetPortalDataByID(portalID);
        SceneData sceneData = sceneDataList.GetSceneDataByID(portalData.sceneID);
        SceneManager.LoadScene(sceneData.sceneName);
        StartCoroutine(CallOnSceneChange());
        Character.Instance.transform.position = portalData.destination;
        StartCoroutine(CameraEffector.Instance.FadeIn());
    }

}
