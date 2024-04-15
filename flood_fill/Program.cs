using CharacterFinder;
// dotnet add package pythonnet
using Python.Runtime;

ImageProcessor.ProcessImage("tests/inqui.png");

public static class NeuralNetwork
{
    public static void Paitu(byte[,] byteArray)
    {
        // python --version
        Runtime.PythonDLL = "python311.dll";
        PythonEngine.Initialize();

        dynamic tf = Py.Import("tensorflow");
        dynamic np = Py.Import("numpy");
        dynamic model = tf.keras.models.load_model("model.keras");
        // pip install pillow
        dynamic list = new PyList();
        list.append(tf.keras.utils.load_img("19.png"));

        Console.WriteLine(list);

        dynamic data = np.array(list);
        dynamic result = model.predict(data);
        Console.WriteLine(result);
        PythonEngine.Shutdown();
    }
}
