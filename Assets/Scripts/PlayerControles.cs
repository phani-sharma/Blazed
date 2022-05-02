using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerControles : MonoBehaviour
{
    [SerializeField] AnimationCurve Curve;
    Rigidbody rb;
    public float speed=5f;
    PhotonView View;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        View = GetComponent<PhotonView>();
        if (View.IsMine)
            Camera.main.transform.SetParent(this.gameObject.transform);
    }
    private void Update()
    {
       if (View.IsMine)
        {
            float X = (Curve.Evaluate(Mathf.Abs(Input.GetAxis("Horizontal")) / 1 * Curve.keys[Curve.length - 1].time)) * (Input.GetAxis("Horizontal") > 0 ? 1 : -1);
            float Y = (Curve.Evaluate(Mathf.Abs(Input.GetAxis("Vertical")) / 1 * Curve.keys[Curve.length - 1].time)) * (Input.GetAxis("Vertical") > 0 ? 1 : -1);
            var velocity = new Vector3(X, 0, Y);

            
            if (Mathf.Abs(X) > 0 && Mathf.Abs(Y) > 0 | velocity.magnitude > 10)
            {
                velocity /= new Vector3(X, 0, Y).magnitude;
            }
            rb.velocity = velocity * speed;
            if (rb.velocity.magnitude < speed * 0.25)
                GetComponent<MeshRenderer>().material.color = Color.green;
            else
            if (rb.velocity.magnitude < speed * 0.75 && rb.velocity.magnitude > speed * 0.25)
                GetComponent<MeshRenderer>().material.color = Color.yellow;
            else
            if (rb.velocity.magnitude > speed * 0.75)
                GetComponent<MeshRenderer>().material.color = Color.red;
        }
    }
}
