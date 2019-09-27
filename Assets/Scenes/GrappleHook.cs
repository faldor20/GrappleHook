using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GrappleHook : MonoBehaviour
{
    class GrapplePoint
    {

        public Vector2 point;
        //this is the vector direction that points from the last grapple point to the new grapplePoint
        public Vector2 releaseLine;
        public float freeRope;
        public bool clockwise;

        public GrapplePoint(Vector2 point, Vector2 releaseLine, float freeRope, bool clockwise)
        {
            this.point = point;

            this.freeRope = freeRope;
            this.clockwise = clockwise;
            this.releaseLine = releaseLine;
        }
    }

    [SerializeField]
    private Stack<GrapplePoint> grapplePoints = new Stack<GrapplePoint>();
    public Rigidbody2D SwingingObject;
    public DistanceJoint2D Rope;
    public LineRenderer ropeGraphics;
    [SerializeField]
    private float ropeLength;
    private int grappleSurfaceLayer;
    public float JumpForce;
    public float sideCheckoffset = 0.0001f;
    // Start is called before the first frame update
    void Start()
    {
        grappleSurfaceLayer = 1 << 8; //here we set the layermask used in raycasting to be that of "grapple surface" layer 8
        ropeGraphics = GetComponent<LineRenderer>();
        Rope = GetComponent<DistanceJoint2D>();

        Rope.autoConfigureDistance = false;
        Rope.maxDistanceOnly = true;
        DoDebugging();
    }

    // Update is called once per frame
    private void FixedUpdate()
    {

        Vector2 freePoint = SwingingObject.transform.position; //this is the point attached to the object that is swinging.

        if (grapplePoints.Count > 0)
        {
            //This is the popint attached to an object in the rope the "hook" end
            DoGrappleChecks(SwingingObject.transform.position, grapplePoints.Peek().point, ropeLength);
            DoDebugging();
        }

    }

    private void Update()
    {

        PlayerInput(SwingingObject.transform.position);
        RenderGraphics(ropeGraphics, SwingingObject.transform.position);
    }

    void DoDebugging()
    {
        var point = grapplePoints.Peek();
        Debug.DrawRay(point.point, point.releaseLine, Color.cyan);
    }
    void RenderGraphics(LineRenderer line, Vector2 freePoint)
    {
        if (line.positionCount > 0) line.SetPosition(line.positionCount - 1, freePoint);
    }

    void PlayerInput(Vector2 freePoint)
    {
        Debug.DrawLine(transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition), Color.red);
        if (Input.GetButtonDown("Fire1"))
        {
            if (grapplePoints.Count > 0) { ClearGrapplePoints(); }
            else
            {
                Vector2 mousepos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var test = Physics2D.Raycast(freePoint, mousepos - freePoint, 100, grappleSurfaceLayer);

                if (test.collider)
                {
                    ClearGrapplePoints();
                    ropeLength = Vector2.Distance(test.point, freePoint);
                    AddGrapplePoint(test.point, freePoint, ropeLength);
                }
            }

        }
        else if (Input.GetButtonDown("Fire2"))
        {

        }
        else if (Input.GetButton("Fire3"))
        {
            /*  var X = Input.GetAxis("Mouse X");
             var Y = Input.GetAxis("Mouse Y");
             var mouseMovement = new Vector2(X, Y);
             SwingingObject.AddForce(((staticPoint - freePoint).normalized + (-mouseMovement)) * JumpForce); */

        }
        else if (Input.GetButtonUp("Fire3"))
        {

        }
    }

    void ChangeRopeLength(float newRopeLength)
    {

    }

    Vector2 GetPointOnRope(Vector2 StaticPoint, Vector2 freePoint)
    {
        Debug.DrawRay(freePoint, (StaticPoint - freePoint).normalized * (Vector2.Distance(freePoint, StaticPoint) - 0.001f), Color.green);
        var test = Physics2D.Raycast(freePoint, StaticPoint - freePoint, Vector2.Distance(freePoint, StaticPoint) - 0.001f, grappleSurfaceLayer);
        if (test.collider)
        {
            var v2Pos = (Vector2) test.transform.position;
            //this moves the point out slightly so that future raycasts dont hit the same object
            var edgeOfObject = (test.point - v2Pos);
            var edgePush = edgeOfObject.normalized * 0.001f;

            Vector2 adjustedPoint = test.point + edgePush;
            return adjustedPoint;
        }
        else { return new Vector2(0, 0); }
    }
    /*
    how do we define the way in which the rope unravels, we can use the velocity, but that may be a little bit dodgy because physicics can be funny.
    What we really are lookig to know is which side of this rope does the object of have just collided with extend, a way to find that is to do a reaycast either side of the rope.
    the cast that hits is the side where there is more object, the cast taht misses is the empty side.
    issues: what if the second cast hits some other object?
    solution , make the casts so close that that wod be very unlikely and add some kind of condidion for if both casts register as hits
     */

    void DoGrappleChecks(Vector2 freePoint, Vector2 staticPoint, float ropeLength)
    {
        var lastPoint = grapplePoints.Peek();

        //This checks if the angle has returned to what it was when the grapple was made meaning the grapple should be reapleased
        if (grapplePoints.Count >= 2)
        {
            var currentAngle = Vector2.SignedAngle(lastPoint.releaseLine, freePoint - lastPoint.point);
            if (lastPoint.clockwise && currentAngle > 0.01f) //This is needed to make sure ti doesn't falsley think the position when made is the position to 
            {
                RemoveGrapplePoint();
                return;
            }
            else if (!lastPoint.clockwise && currentAngle < -0.01f)
            {
                RemoveGrapplePoint();
                return;
            }
        }

        lastPoint = grapplePoints.Peek(); //we now have to update what our lastPoint is
        Vector2 newPoint = GetPointOnRope(grapplePoints.Peek().point, freePoint);
        if (newPoint != Vector2.zero)
        {
            AddGrapplePoint(newPoint, freePoint, ropeLength);

        }
    }

    void AddGrapplePoint(Vector2 newGrapplePoint, Vector2 freePoint /* , Vector2 lastGrapplePoint */ , float ropeLength)
    { //TODO: i need to store the current rope length and then make a new one relative to this new point

        ropeGraphics.positionCount = (ropeGraphics.positionCount < 2) ? 2 : ropeGraphics.positionCount + 1; //there can never be less than two points for ropegraphics becauase there must allways be a static and swinging point;

        ropeGraphics.SetPosition(ropeGraphics.positionCount - 2, newGrapplePoint); //We are getting the second to last because the last will allways be the players position
        //float angleBetweenFreeAndStatic = Vector2.SignedAngle(new Vector2(0, 1), newGrapplePoint - freePoint);
        float clockwiseAngle = 0;
        float freeRope = ropeLength;
        bool goingClockwise = false;
        if (grapplePoints.Count > 0)
        {
            Vector2 castDirection = newGrapplePoint - freePoint;

            var results = CastToEitherSide(freePoint, castDirection, Vector2.Distance(freePoint, newGrapplePoint) + 0.02f, sideCheckoffset);

            if (results[0].collider && results[0].collider) Debug.LogError("Both left and right casts hit, increase offset till this stops happening");
            else if (!results[0].collider && !results[0].collider) Debug.LogError("neither left nor right casts hit,adjust offfset");
            if (results[0].collider)
            {
                goingClockwise = true;
            }
            else if (results[1].collider)
            {
                goingClockwise = false;
            }

        }
        grapplePoints.Push(new GrapplePoint(newGrapplePoint, freePoint - newGrapplePoint, freeRope, goingClockwise));
        SetupGrappleJoint(newGrapplePoint);
    }
    /// <summary>
    /// A simple utilit function for doing raycast check either side of a vector
    /// Checks Right then left
    /// </summary>
    private RaycastHit2D[] CastToEitherSide(Vector2 origin, Vector2 castDirection, float castDistance, float offsetAmount)
    {
        //Adding a perpendicular vector to an existing vector will move it right, negative will move it left.
        Vector2 offset = Vector2.Perpendicular(castDirection.normalized) * offsetAmount;

        RaycastHit2D[] results = new RaycastHit2D[2];

        for (int i = 0; i < 2; i++)
        {
            //The second cast will be offset in the opposite direction
            if (i == 1) offset = (-offset);
            results[i] = Physics2D.Raycast(origin, castDirection + offset, castDistance, grappleSurfaceLayer);
        }
        return results;
    }

    void ClearGrapplePoints()
    {
        grapplePoints.Clear();
        ropeGraphics.positionCount = 0;
        DisableRopeConnection();
    }
    void RemoveGrapplePoint()
    {
        ropeGraphics.positionCount--;
        grapplePoints.Pop();
        if (grapplePoints.Count > 0) SetupGrappleJoint(grapplePoints.Peek().point);
        else DisableRopeConnection();
    }
    void DisableRopeConnection()
    {
        Rope.enabled = false;
    }
    void SetupGrappleJoint(Vector2 staticPoint)
    {
        if (!Rope.enabled) Rope.enabled = true;
        Rope.distance = grapplePoints.Peek().freeRope;
        Rope.connectedAnchor = staticPoint;

        /* var distanceToFreePoint = Vector2.Distance(freePoint, staticPoint);
        var maxDistanceFromlastGrapplePoint = maxDistance - grapplePoints.Peek().ropeLengthBeforePoint;
        if (distanceToFreePoint > maxDistanceFromlastGrapplePoint) //if i don;'t use the distance function this can all be done a little quicker
            swingingTrans.position = (((freePoint - staticPoint).normalized * maxDistanceFromlastGrapplePoint) + staticPoint); //we add teh static point back to convert from local space of the static point into world */

    }
}