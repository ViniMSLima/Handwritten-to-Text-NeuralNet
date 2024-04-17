import numpy as np
import tensorflow as tf
import sys

# Load the pre-trained model
model = tf.keras.models.load_model("C:/Users/disrct/Desktop/VC_Projeto/train/bestModels/90_88_58ms.keras")

maybeResults = ["0","1","2","3","4","5","6","7","8","9",
                "a","b","c","d","e","f","g","h","i","j",
                "k","l","m","n","o","p","q","r","s","t",
                "u","v","w","x","y","z","A","B","C","D",
                "E","F","G","H","I","J","K","L","M","N",
                "O","P","Q","R","S","T","U","V","W","X",
                "Y","Z"]

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
        a += f"{maybeResults[np.argmax(wordResults[i][0])]}"
    
    # Write the result to stdout
    sys.stdout.write(a)
    sys.stdout.flush()

if __name__ == "__main__":
    main()
