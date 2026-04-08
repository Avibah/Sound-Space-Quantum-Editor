using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace SSQE_Player.Models
{
    internal class ModelManager
    {
        private static readonly List<int> VaOs = new();
        private static readonly List<int> VbOs = new();

        private readonly Dictionary<string, Model> models = new();
        private static readonly Dictionary<string, (int, int)> instancedHandles = new();

        public void RegisterModel(string name, float[] vertices, float scale)
        {
            Model model = LoadModelToVao(vertices, scale);

            models.Add(name, model);
            instancedHandles.Add(name, (VbOs[^2], VbOs[^1]));
        }

        public Model GetModel(string name)
        {
            return models[name];
        }

        public static Model LoadModelToVao(float[] vertices, float scale)
        {
            int vao = CreateVao();
            StoreDataInAttribList(vertices);

            return new Model(vertices, scale, vao);
        }

        private static int CreateVao()
        {
            int vao = GL.GenVertexArray();
            VaOs.Add(vao);

            GL.BindVertexArray(vao);

            return vao;
        }

        private static void StoreDataInAttribList(float[] data)
        {
            int vbo = GL.GenBuffer();
            VbOs.Add(vbo);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, data.Length * sizeof(float), data, BufferUsage.StaticDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            int instancedvbo1 = GL.GenBuffer();
            VbOs.Add(instancedvbo1);
            int instancedvbo2 = GL.GenBuffer();
            VbOs.Add(instancedvbo2);

            GL.BindBuffer(BufferTarget.ArrayBuffer, instancedvbo1);

            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribDivisor(1, 1);

            GL.BindBuffer(BufferTarget.ArrayBuffer, instancedvbo2);

            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribDivisor(2, 1);
        }

        public void Render(Vector3[] positions, Vector4[] colors, int count, string name)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, instancedHandles[name].Item1);
            GL.BufferData(BufferTarget.ArrayBuffer, positions.Length * 3 * sizeof(float), positions, BufferUsage.DynamicDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, instancedHandles[name].Item2);
            GL.BufferData(BufferTarget.ArrayBuffer, colors.Length * 4 * sizeof(float), colors, BufferUsage.DynamicDraw);

            GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, models[name].vertexCount, count);
        }
    }
}
