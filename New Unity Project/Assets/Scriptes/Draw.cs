using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Draw : MonoBehaviour
{
    private List<GameObject> points;
    private List<Vector3> positions;
    private const int countOfPoints = 5;
    private Camera camera;
    public GameObject line1;
    public GameObject line2;
    float delta = 0.1f;

    void Start()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        line1 = Instantiate(line1);
        line2 = Instantiate(line2);
        camera = GameObject.Find("MainCamera").GetComponent<Camera>();
        positions = new List<Vector3>();
        positions.Add(new Vector3(-14, -5, 0));
        positions.Add(new Vector3(14, 0, 0));
        points = new List<GameObject>();
        int k = 5;
        for (int i = 0; i < countOfPoints; i++)
        {
            Vector3 pos = new Vector3(k - 16, Random.Range(-5, 5), 0);
            positions.Add(pos);
            points.Add(Instantiate(Resources.Load("Point", typeof(GameObject)) as GameObject, pos, Quaternion.identity));
            k += 5;
        }
        positions = positions.OrderBy(v => v.x).ToList<Vector3>();
        DrawLines();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0))
        {
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            Vector3 position = ray.origin + ray.direction;
            float minLen = Mathf.Abs(position.x - points[0].transform.position.x);
            int index = 0;
            for (int i = 1; i < countOfPoints; i++)
            {
                float len = Mathf.Abs(position.x - points[i].transform.position.x);
                if (len < minLen)
                {
                    minLen = len;
                    index = i;
                }
            }
            Vector3 newPos = new Vector3(points[index].transform.position.x, position.y, 0);
            points[index].transform.position = newPos;
            positions[index + 1] = newPos;
            DrawLines();
        }

    }
    

    void DrawLines()
    {
        LineRenderer lr = line1.GetComponent<LineRenderer>();
        List<Vector3> lagPoints = Lagrange();
        lr.positionCount = lagPoints.Count;
        lr.startWidth = 0.25f;
        for (int i = 0; i < lagPoints.Count; i++)
        {
            lr.SetPosition(i, lagPoints[i]);
        }

        LineRenderer lr2 = line2.GetComponent<LineRenderer>();
        List<Vector3> olsPoints = OLS();
        lr2.positionCount = olsPoints.Count;
        lr2.startWidth = 0.25f;
        for (int i = 0; i < olsPoints.Count; i++)
        {
            lr2.SetPosition(i, olsPoints[i]);
        }
    }

    List<Vector3> Lagrange()
    {
        List<Vector3> result = new List<Vector3>();
        for (int i = 0; i < positions.Count; i++)
        {
            result.Add(positions[i]);
        }
        for (int i = 0; i < positions.Count - 1; i++)
        {
            for (float x = positions[i].x + delta; x < positions[i + 1].x; x += delta)
            {
                float y = 0;
                for (int j = 0; j < positions.Count; j++)
                {
                    float l = 1;
                    for (int k = 0; k < positions.Count; k++)
                    {
                        if (k == j) continue;
                        l *= (x - result[k].x) / (result[j].x - result[k].x);
                    }
                    y += l * result[j].y;
                }
                result.Add(new Vector3(x, y, 0));
            }
        }

        result = result.OrderBy(v => v.x).ToList<Vector3>();
        return result;
    }

    List<float> FillSums()
    {
        List<float> sums = new List<float>();
        sums.Add(positions.Count);
        for (int i = 1; i <= 6; i++)
        {
            float sum = 0;
            for (int j = 0; j < positions.Count; j++)
            {
                sum += Mathf.Pow(positions[j].x, i);
            }
            sums.Add(sum);
        }
        return sums;
    }

    List<float> FillSumsForY()
    {
        List<float> sumsy = new List<float>();
        for (int i = 0; i <= 3; i++)
        {
            float sum = 0;
            for (int j = 0; j < positions.Count; j++)
            {
                sum += positions[j].y * Mathf.Pow(positions[j].x, i);
            }
            sumsy.Add(sum);
        }
        return sumsy;
    }

    List<List<float>> CreateMatrix()
    {
        List<List<float>> matrix = new List<List<float>>();
        List<float> sums = FillSums();
        List<float> sumsy = FillSumsForY();

        for (int i = 0; i <= 3; i++)
        {
            matrix.Add(new List<float>());
            for (int j = 0; j <= 3; j++)
            {
                matrix[i].Add(sums[(i + j)]);
            }
            matrix[i].Add(sumsy[i]);
        }

        return matrix;
    }

    List<float> Gause(List<List<float>> matrix)
    {
        List<float> result = new List<float>();

        for (int i = 0; i < matrix.Count; i++)
        {
            float main = matrix[i][i];
            for (int j = 0; j < matrix[i].Count; j++)
            {
                matrix[i][j] /= main;
            }

            for (int j = 0; j < matrix.Count; j++)
            {
                if (j == i) continue;
                main = matrix[j][i];
                for (int k = 0; k < matrix[j].Count; k++)
                {
                    matrix[j][k] -= main * matrix[i][k];
                }
            }
        }

        for (int i = 0; i < matrix.Count; i++)
        {
            result.Add(matrix[i][matrix[i].Count - 1]);
        }

        return result;
    }

    List<Vector3> OLS()
    {
        List<Vector3> result = new List<Vector3>();
        List<float> coef = Gause(CreateMatrix());

        for (float x = positions[0].x; x <= positions[positions.Count - 1].x; x += delta)
        {
            float y = 0;
            for (int i = 0; i < coef.Count; i++)
            {
                y += coef[i] * Mathf.Pow(x, i);
            }
            result.Add(new Vector3(x, y, 0));
        }

        return result;
    }
}
