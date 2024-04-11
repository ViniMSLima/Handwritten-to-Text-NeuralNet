import os
import csv
import string
import pandas as pd

def datasetOrganizer(csv_file, img_folder):
    df = pd.read_csv(csv_file, header=None)
    
    for index, row in df.iterrows():
        img_path, label = row[0], str(row[1])
        
        if os.path.exists(img_path):
            if label.isdigit():
                label_folder = os.path.join(img_folder, label)
            elif label.isalpha():
                if label.islower():
                    label_folder = os.path.join(img_folder, "lower" + label.upper())
                else:
                    label_folder = os.path.join(img_folder, "upper" + label)
            
            if not os.path.exists(label_folder):
                os.makedirs(label_folder)
                
            new_img_path = os.path.join(label_folder, os.path.basename(img_path))
            os.rename(img_path, new_img_path)

csv_file = "english.csv"
img_folder = "Img"
datasetOrganizer(csv_file, img_folder)
