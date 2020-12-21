using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapCamera : SingletonMonoBehaviour<MinimapCamera>
{
    public Camera camera { get; private set; }
    public Camera minimapIconCamera;
    public Transform targetObject;
    public float followSpeed;
    //public float quickThreshold = 20f;
    public float stopThreshold = 0.1f;

    new void Awake()
    {
        base.Awake();

        camera = GetComponent<Camera>();
        minimapIconCamera.orthographicSize = camera.orthographicSize;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        FollowPlayer();
    }

    void FollowPlayer()
    {
        Vector3 targetPos = transform.position;
        Quaternion targetRot = transform.rotation;
        Vector3 targetUp = transform.up;

        /*if (GameManager.Instance.playerModel)
        {
            targetPos = GameManager.Instance.playerModel.transform.position;
        }*/
        targetPos = targetObject.position;
        targetPos.y = transform.position.y;

        targetUp = CinemachineCameraManager.Instance.brain.OutputCamera.transform.forward;
        targetUp.y = 0;

        if (Vector3.Distance(transform.position, targetPos) <= stopThreshold)
        {
            transform.position = targetPos;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
        }

        transform.LookAt(transform.position + Vector3.down * 5, targetUp);
    }
}
