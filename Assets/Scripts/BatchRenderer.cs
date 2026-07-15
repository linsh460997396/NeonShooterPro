using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace NeonShooter
{
    /// <summary>
    /// Unity版SpriteBatch:使用GL立即模式把所有同纹理的四边形批处理成单次GPU调用。
    /// 坐标系与GUI一致:(0,0)=屏幕左上角,Y向下递增。
    /// 所有渲染使用Additive混合,与原版MonoGame NeonShooter一致。
    /// </summary>
    public class BatchRenderer : MonoBehaviour
    {
        public static BatchRenderer Instance { get; private set; }

        Material material;

        struct Quad
        {
            public Vector2 Position;
            public Vector2 Origin;
            public float Rotation;
            public Vector2 Scale;
            public Color Color;
        }

        class Batch
        {
            public Texture Texture;
            public List<Quad> Quads = new List<Quad>();
        }

        Dictionary<Texture, Batch> batches = new Dictionary<Texture, Batch>();
        List<Batch> batchList = new List<Batch>();

        void Awake()
        {
            Instance = this;
            var shader = Shader.Find("NeonShooter/BatchAdditive");
            if (shader == null)
                shader = Shader.Find("Sprites/Default");
            material = new Material(shader);
        }

        void OnEnable()
        {
            StartCoroutine(RenderAtEndOfFrame());
        }

        IEnumerator RenderAtEndOfFrame()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                RenderBatched();
            }
        }

        /// <summary>
        /// 绘制一个纹理四边形(原点为纹理中心偏移)。
        /// </summary>
        public void Draw(Texture2D texture, Vector2 position, Vector2 origin, float rotation, Vector2 scale, Color color)
        {
            if (texture == null) return;

            if (!batches.TryGetValue(texture, out var batch))
            {
                batch = new Batch { Texture = texture };
                batches[texture] = batch;
                batchList.Add(batch);
            }

            batch.Quads.Add(new Quad
            {
                Position = position,
                Origin = origin,
                Rotation = rotation,
                Scale = scale,
                Color = color
            });
        }

        public void Draw(Texture2D texture, Vector2 position, Vector2 origin, float rotation, float scale, Color color)
        {
            Draw(texture, position, origin, rotation, new Vector2(scale, scale), color);
        }

        /// <summary>
        /// 绘制一个纹理四边形,原点为纹理中心。
        /// </summary>
        public void Draw(Texture2D texture, Vector2 position, float rotation, Vector2 scale, Color color)
        {
            if (texture == null) return;
            Draw(texture, position, new Vector2(texture.width * 0.5f, texture.height * 0.5f), rotation, scale, color);
        }

        public void Draw(Texture2D texture, Vector2 position, float rotation, float scale, Color color)
        {
            Draw(texture, position, rotation, new Vector2(scale, scale), color);
        }

        /// <summary>
        /// 用1x1像素纹理画线(等价于SpriteBatch.DrawLine)。
        /// </summary>
        public void DrawLine(Texture2D pixel, Vector2 start, Vector2 end, Color color, float thickness = 2f)
        {
            Vector2 delta = end - start;
            float length = delta.magnitude;
            if (length < 0.0001f) return;
            float angle = Mathf.Atan2(delta.y, delta.x);
            // 1x1像素纹理,原点为(0, 0.5)即左中,BatchRenderer内部乘以scale后得到(0, thickness/2)
            Draw(pixel, start, new Vector2(0f, 0.5f), angle, new Vector2(length, thickness), color);
        }

        // 在WaitForEndOfFrame后调用,此时所有相机渲染完毕,可直接用GL绘制到屏幕
        void RenderBatched()
        {
            GL.PushMatrix();
            // 使用LoadOrtho:坐标范围0~1, (0,0)=左下角
            GL.LoadOrtho();

            float invW = 1f / Screen.width;
            float invH = 1f / Screen.height;

            foreach (var batch in batchList)
            {
                if (batch.Quads.Count == 0) continue;

                material.mainTexture = batch.Texture;
                material.SetPass(0);

                GL.Begin(GL.QUADS);

                float texW = batch.Texture.width;
                float texH = batch.Texture.height;
                var quads = batch.Quads;
                for (int i = 0; i < quads.Count; i++)
                {
                    var q = quads[i];
                    float cos = Mathf.Cos(q.Rotation);
                    float sin = Mathf.Sin(q.Rotation);

                    float w = texW * q.Scale.x;
                    float h = texH * q.Scale.y;
                    float ox = q.Origin.x * q.Scale.x;
                    float oy = q.Origin.y * q.Scale.y;

                    // 四个角(相对原点,未旋转,屏幕像素坐标)
                    float x0 = -ox, y0 = -oy;
                    float x1 = w - ox, y1 = -oy;
                    float x2 = w - ox, y2 = h - oy;
                    float x3 = -ox, y3 = h - oy;

                    // 旋转 + 平移(屏幕像素坐标)
                    float sx0 = x0 * cos - y0 * sin + q.Position.x;
                    float sy0 = x0 * sin + y0 * cos + q.Position.y;
                    float sx1 = x1 * cos - y1 * sin + q.Position.x;
                    float sy1 = x1 * sin + y1 * cos + q.Position.y;
                    float sx2 = x2 * cos - y2 * sin + q.Position.x;
                    float sy2 = x2 * sin + y2 * cos + q.Position.y;
                    float sx3 = x3 * cos - y3 * sin + q.Position.x;
                    float sy3 = x3 * sin + y3 * cos + q.Position.y;

                    // 屏幕像素坐标 → NDC (0~1, Y翻转:屏幕Y向下→NDC Y向上)
                    GL.Color(q.Color);

                    GL.TexCoord2(0f, 1f);
                    GL.Vertex3(sx0 * invW, 1f - sy0 * invH, 0f);
                    GL.TexCoord2(1f, 1f);
                    GL.Vertex3(sx1 * invW, 1f - sy1 * invH, 0f);
                    GL.TexCoord2(1f, 0f);
                    GL.Vertex3(sx2 * invW, 1f - sy2 * invH, 0f);
                    GL.TexCoord2(0f, 0f);
                    GL.Vertex3(sx3 * invW, 1f - sy3 * invH, 0f);
                }

                GL.End();
            }

            GL.PopMatrix();

            // 清空所有批处理,下一帧重新填充
            for (int i = 0; i < batchList.Count; i++)
                batchList[i].Quads.Clear();
        }
    }
}
