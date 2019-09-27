using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeCreator : MonoBehaviour
{

    public float pointsPerUnit;
    public float length;
    public GameObject ropePointPrefab;
    public Rigidbody2D firstConnection;
    public Rigidbody2D lastConnection;
    public Rigidbody2D[] ropeParts;
    public LineRenderer ropeLine;
    public bool distanceJoint;
    [SerializeField]
    private bool doneMain;
    // Start is called before the first frame update
    private void Start()
    {
        if (distanceJoint) MakeRopesDistance();
        else MakeRopesHinge();
    }

    void MakeRopesHinge()
    {
        doneMain = true;
        Debug.Log("hey");
        int numberOfPoints = (int) (length * pointsPerUnit);
        Vector2 startpoint = firstConnection.transform.position;
        Vector2 endPoint = lastConnection.transform.position;
        ropeParts = new Rigidbody2D[numberOfPoints];
        float distanceBetweenPoints = length / numberOfPoints;
        for (int i = 0; i < numberOfPoints; i++)
        {

            //TODO: use prefabs to see if it performs faster
            Vector2 pointLocation = Vector2.Lerp(startpoint, endPoint, (float) (i + 1) / (float) numberOfPoints);
            GameObject point = Instantiate(ropePointPrefab, pointLocation, Quaternion.identity);
            var springJoint = point.GetComponent<HingeJoint2D>();
            var rb = point.GetComponent<Rigidbody2D>();
            ropeParts[i] = rb;

            Rigidbody2D lastRope;

            if (i == 0) lastRope = firstConnection;
            else lastRope = ropeParts[i - 1];

            springJoint.connectedBody = lastRope;
            springJoint.connectedAnchor = lastRope.transform.position - rb.transform.position;
        }

        ropeLine.positionCount = numberOfPoints;
    }
    void MakeRopesDistance()
    {
        doneMain = true;
        Debug.Log("hey");
        int numberOfPoints = (int) (length * pointsPerUnit);
        Vector2 startpoint = firstConnection.transform.position;
        Vector2 endPoint = lastConnection.transform.position;
        ropeParts = new Rigidbody2D[numberOfPoints];
        float distanceBetweenPoints = length / numberOfPoints;
        for (int i = 0; i < numberOfPoints; i++)
        {

            //TODO: use prefabs to see if it performs faster
            Vector2 pointLocation = Vector2.Lerp(startpoint, endPoint, (float) (i + 1) / (float) numberOfPoints);
            GameObject point = Instantiate(ropePointPrefab, pointLocation, Quaternion.identity);
            var springJoint = point.GetComponent<DistanceJoint2D>();
            var rb = point.GetComponent<Rigidbody2D>();
            ropeParts[i] = rb;
            if (i == 0) springJoint.connectedBody = firstConnection;
            else springJoint.connectedBody = ropeParts[i - 1];

            springJoint.distance = length / numberOfPoints;
        }

        ropeLine.positionCount = numberOfPoints;
    }
    // Update is called once per frame
    void Update()
    {
        var ropePositions = new Vector3[ropeParts.Length];
        for (int i = 0; i < ropeParts.Length; i++)
        {
            ropePositions[i] = ropeParts[i].transform.position;
        }

        ropeLine.SetPositions(ropePositions);

    }
}