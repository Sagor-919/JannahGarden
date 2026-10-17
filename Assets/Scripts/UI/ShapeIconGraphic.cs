using UnityEngine;
using UnityEngine.UI;

namespace JannahGarden.UI
{
    public sealed class ShapeIconGraphic : Graphic
    {
        public enum IconShape
        {
            Pause,
            Play,
            Settings,
            Restart
        }

        [SerializeField] private IconShape shape = IconShape.Pause;

        public IconShape Shape
        {
            get { return shape; }
            set
            {
                if (shape == value)
                    return;

                shape = value;
                SetVerticesDirty();
            }
        }

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();
            Rect rect = GetPixelAdjustedRect();
            if (rect.width <= 0f || rect.height <= 0f)
                return;

            if (shape == IconShape.Pause)
            {
                AddRectangle(vertexHelper, rect, 0.24f, 0.18f, 0.40f, 0.82f);
                AddRectangle(vertexHelper, rect, 0.60f, 0.18f, 0.76f, 0.82f);
            }
            else if (shape == IconShape.Play)
            {
                AddTriangle(vertexHelper, rect, new Vector2(0.30f, 0.16f), new Vector2(0.30f, 0.84f), new Vector2(0.82f, 0.50f));
            }
            else if (shape == IconShape.Settings)
            {
                AddGear(vertexHelper, rect);
            }
            else
            {
                AddRestartArrow(vertexHelper, rect);
            }
        }

        private void AddRestartArrow(VertexHelper vertexHelper, Rect rect)
        {
            const int segments = 20;
            const float start = -45f * Mathf.Deg2Rad;
            const float end = 220f * Mathf.Deg2Rad;
            Vector2 center = new Vector2(0.5f, 0.5f);
            for (int i = 0; i < segments; i++)
            {
                float a = Mathf.Lerp(start, end, (float)i / segments);
                float b = Mathf.Lerp(start, end, (float)(i + 1) / segments);
                Vector2 from = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                Vector2 to = new Vector2(Mathf.Cos(b), Mathf.Sin(b));
                AddQuad(vertexHelper, rect,
                    center + from * 0.25f, center + from * 0.36f,
                    center + to * 0.36f, center + to * 0.25f);
            }

            Vector2 radial = new Vector2(Mathf.Cos(end), Mathf.Sin(end));
            Vector2 tangent = new Vector2(-radial.y, radial.x);
            Vector2 basePoint = center + radial * 0.305f;
            AddTriangle(vertexHelper, rect,
                basePoint + radial * 0.13f,
                basePoint - radial * 0.13f,
                basePoint + tangent * 0.20f);
        }

        private void AddGear(VertexHelper vertexHelper, Rect rect)
        {
            const int teeth = 8;
            for (int i = 0; i < teeth; i++)
            {
                float angle = i * Mathf.PI * 2f / teeth;
                Vector2 outward = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Vector2 tangent = new Vector2(-outward.y, outward.x);
                Vector2 center = new Vector2(0.5f, 0.5f) + outward * 0.39f;
                Vector2 halfWidth = tangent * 0.085f;
                Vector2 inner = outward * 0.28f;
                Vector2 outer = outward * 0.5f;
                AddQuad(vertexHelper, rect,
                    center - halfWidth + (inner - outward * 0.39f),
                    center + halfWidth + (inner - outward * 0.39f),
                    center + halfWidth + (outer - outward * 0.39f),
                    center - halfWidth + (outer - outward * 0.39f));
            }

            const int segments = 32;
            int start = vertexHelper.currentVertCount;
            AddVertex(vertexHelper, rect, new Vector2(0.5f, 0.5f), color);
            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                Vector2 point = new Vector2(0.5f + Mathf.Cos(angle) * 0.34f, 0.5f + Mathf.Sin(angle) * 0.34f);
                AddVertex(vertexHelper, rect, point, color);
            }
            for (int i = 0; i < segments; i++)
                vertexHelper.AddTriangle(start, start + 1 + i, start + 1 + (i + 1) % segments);

            int centerStart = vertexHelper.currentVertCount;
            AddVertex(vertexHelper, rect, new Vector2(0.5f, 0.5f), Color.clear);
            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                Vector2 point = new Vector2(0.5f + Mathf.Cos(angle) * 0.14f, 0.5f + Mathf.Sin(angle) * 0.14f);
                AddVertex(vertexHelper, rect, point, Color.clear);
            }
            for (int i = 0; i < segments; i++)
                vertexHelper.AddTriangle(centerStart, centerStart + 1 + i, centerStart + 1 + (i + 1) % segments);
        }

        private void AddQuad(VertexHelper vertexHelper, Rect rect, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            int start = vertexHelper.currentVertCount;
            AddVertex(vertexHelper, rect, a, color);
            AddVertex(vertexHelper, rect, b, color);
            AddVertex(vertexHelper, rect, c, color);
            AddVertex(vertexHelper, rect, d, color);
            vertexHelper.AddTriangle(start, start + 1, start + 2);
            vertexHelper.AddTriangle(start, start + 2, start + 3);
        }

        private void AddVertex(VertexHelper vertexHelper, Rect rect, Vector2 point, Color vertexColor)
        {
            vertexHelper.AddVert(new Vector3(rect.xMin + rect.width * point.x, rect.yMin + rect.height * point.y), vertexColor, Vector2.zero);
        }

        private void AddRectangle(VertexHelper vertexHelper, Rect rect, float xMin, float yMin, float xMax, float yMax)
        {
            AddTriangle(vertexHelper, rect, new Vector2(xMin, yMin), new Vector2(xMin, yMax), new Vector2(xMax, yMax));
            AddTriangle(vertexHelper, rect, new Vector2(xMin, yMin), new Vector2(xMax, yMax), new Vector2(xMax, yMin));
        }

        private void AddTriangle(VertexHelper vertexHelper, Rect rect, Vector2 a, Vector2 b, Vector2 c)
        {
            int start = vertexHelper.currentVertCount;
            Color32 vertexColor = color;
            vertexHelper.AddVert(new Vector3(rect.xMin + rect.width * a.x, rect.yMin + rect.height * a.y), vertexColor, Vector2.zero);
            vertexHelper.AddVert(new Vector3(rect.xMin + rect.width * b.x, rect.yMin + rect.height * b.y), vertexColor, Vector2.zero);
            vertexHelper.AddVert(new Vector3(rect.xMin + rect.width * c.x, rect.yMin + rect.height * c.y), vertexColor, Vector2.zero);
            vertexHelper.AddTriangle(start, start + 1, start + 2);
        }
    }
}
