using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;

public class MLUnit : Agent
{
    [SerializeField] private Vector2[] startPositionRange;
    [SerializeField] private Transform[] targetTransforms;
    [SerializeField] private Player inputPlayer;
    [SerializeField] private RenderTexture baseRenderTexture;

    private Unit unit;
    private Vector2 target;
    private int targetIndex = 0;

    private const float timeToCheckpoint = 2.0f;
    private float timer = 0.0f;
    private float totalTime = 0.0f;

    private float avgTime = 0.0f;
    private int failCount = 0;
    private int winCount = 0;

    private void Awake() {
        unit = GetComponent<Unit>();
        RenderTexture rt = new RenderTexture(baseRenderTexture);
        rt.filterMode = FilterMode.Point;
        GetComponentInChildren<Camera>().targetTexture = rt;
        GetComponentInChildren<Camera>().enabled = true;
        GetComponent<RenderTextureSensorComponent>().RenderTexture = rt;
    }

    private Vector2 GetStartPosition()
    {
        Vector2 offset = startPositionRange[1] - startPositionRange[0];
        return startPositionRange[0] + (offset * Random.Range(0.0f, 1.0f));
    }

    public override void OnEpisodeBegin()
    {
        return;
        Debug.Log(GetComponent<BehaviorParameters>().Model.name + ": FAILS(" + failCount + "), AVG(" + avgTime + ")");
        transform.position = GetStartPosition();
        targetIndex = 0;
        target = targetTransforms[0].position;
        timer = timeToCheckpoint;
        totalTime = 0.0f;
        unit.data.rb.velocity = Vector2.zero;
        unit.stateMachine.Reset();
        unit.data.input.running = true;
    }
    
    private void Update() {
        Log.Text("ML" + unit.ID, GetComponent<BehaviorParameters>().Model.name, Camera.main.WorldToScreenPoint(transform.position), Color.green, Time.deltaTime);
        Debug.DrawLine(transform.position, target, Color.blue);
        timer -= Time.deltaTime;
        totalTime += Time.deltaTime;
        if (timer <= 0.0f)
        {
            failCount++;
            SetReward(-2);
            EndEpisode();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(target - (Vector2)transform.position);
        sensor.AddObservation(unit.data.rb.velocity);
        //sensor.AddObservation(target);
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
    
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.transform == targetTransforms[targetIndex])
        {
            //AddReward(1);
            targetIndex++;
            timer = timeToCheckpoint;
            if (targetIndex == targetTransforms.Length)
            {
                winCount++;
                avgTime += totalTime / winCount;
                SetReward(30 - totalTime);
                EndEpisode();
                return;
            }
            target = targetTransforms[targetIndex].position;
        }
    }
    
}
