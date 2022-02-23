using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;

public class MLUnit : Agent
{
    [SerializeField] private Transform routesParent;
    [SerializeField] private Player inputPlayer;
    [SerializeField] private RenderTexture baseRenderTexture;

    private Unit unit;
    private MLRoute[] routes;
    private Vector2 target;
    private int routeIndex = 0;
    private float initialDistance = 0.0f;

    private const float timeToCheckpoint = 5.0f;
    private float timer = 0.0f;

    private void Awake() {
        unit = GetComponent<Unit>();
        RenderTexture rt = new RenderTexture(baseRenderTexture);
        rt.filterMode = FilterMode.Point;
        GetComponentInChildren<Camera>().targetTexture = rt;
        GetComponentInChildren<Camera>().enabled = true;
        GetComponent<RenderTextureSensorComponent>().RenderTexture = rt;
        routes = routesParent.GetComponentsInChildren<MLRoute>();
    }

    public override void OnEpisodeBegin()
    {
        //Debug.Log(GetComponent<BehaviorParameters>().Model.name + ": FAILS(" + failCount + "), AVG(" + avgTime + ")");
        routeIndex = Random.Range(0, routes.Length);
        transform.position = routes[routeIndex].GetStart();
        target = routes[routeIndex].GetEnd();
        initialDistance = Vector2.Distance(transform.position, target);
        timer = 0.0f;
        unit.data.rb.velocity = Vector2.zero;
        unit.stateMachine.Reset();
        unit.data.input.running = true;
    }
    
    private void Update() {
        //Log.Text("ML" + unit.ID, GetComponent<BehaviorParameters>().Model.name, Camera.main.WorldToScreenPoint(transform.position), Color.green, Time.deltaTime);
        Debug.DrawLine(transform.position, target, Color.blue);
        
        if (Vector2.Distance(transform.position, target) <= 0.5f)
        {
            //routeIndex++;
            //if (routeIndex == routes.Length) routeIndex = 0;
            SetReward(initialDistance);
            EndEpisode();
            return;
        }
        
        timer += Time.deltaTime;
        if (timer >= timeToCheckpoint)
        {
            SetReward(initialDistance - Vector2.Distance(transform.position, target));
            EndEpisode();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(target - (Vector2)transform.position);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Move
        unit.data.input.movement = actionBuffers.DiscreteActions[0] - 1;
        // Jump
        if (actionBuffers.DiscreteActions[1] == 1)
        {
            unit.data.input.jumpRequestTime = Time.unscaledTime;
        }
        // Crawl
        if (actionBuffers.DiscreteActions[2] == 1)
        {
            unit.data.input.crawlRequestTime = Time.unscaledTime;
        }
    }
    
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = Mathf.RoundToInt(inputPlayer.data.input.movement) + 1;
        discreteActions[1] = inputPlayer.data.input.jumpQueued ? 1 : 0;
        discreteActions[2] = inputPlayer.data.input.crawling ? 1 : 0;
    }
    
}
