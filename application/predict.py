import numpy as np
import tensorflow as tf
import sys

# Load the pre-trained model
model = tf.keras.models.load_model("C:/Users/disrct/Desktop/VC_Projeto/train/bestModels/modelTest.keras")

def main():
    nImages = int(sys.argv[1])
    
    wordResults = []
    
    for i in range(nImages):    
        img = "C:/Users/disrct/Desktop/VC_Projeto/application/" + sys.argv[i + 2]
        data = np.array([tf.keras.utils.load_img(img)])
           
        # Perform prediction using the loaded model
        wordResults.append(model.predict(data))
    
    a = ""    
    for i in range(nImages):
        a += f"resultado: {str(np.argmax(wordResults[i][0]))} : {str(np.amax(wordResults[i][0]))};\n"
    
    # Write the result to stdout
    sys.stdout.write(a)
    sys.stdout.flush()

if __name__ == "__main__":
    main()
