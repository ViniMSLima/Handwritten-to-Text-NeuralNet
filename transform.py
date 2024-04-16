import os
import cv2 as cv
import numpy as np
import matplotlib.pyplot as plt
from random import randint
from PIL import Image

directory = 'datasets/'
directory1 = 'datasets/'
numeros = [str(i) for i in range(10)]
letras = ["upper" + chr(i) for i in range(65, 91)]
letras_minusculas = ["lower" + chr(i) for i in range(65, 91)]

pastas = numeros + letras + letras_minusculas

if not os.path.exists('datasets/ds'):
    os.mkdir("datasets/ds")

for pasta in pastas:
    if not os.path.exists(f"datasets/ds/{pasta}"):
        os.mkdir(f"datasets/ds/{pasta}")

kernel_dilation = np.ones((40, 40), np.uint8)
kernel_erosion = np.ones((25, 25), np.uint8)

def resize_and_process_image(image_path, output_dir, filename, size=(128, 128)):
    img = cv.imread(image_path, cv.IMREAD_GRAYSCALE)
    
    img_resized = cv.resize(img, size)
    cv.imwrite(os.path.join(output_dir, f"normal_{filename}"), img_resized)
    
    img_dilation = cv.dilate(img, kernel_dilation)
    img_dilation_resized = cv.resize(img_dilation, size)
    cv.imwrite(os.path.join(output_dir, f"dilatation_{filename}"), img_dilation_resized)
    
    img_erosion = cv.erode(img, kernel_erosion)
    img_erosion_resized = cv.resize(img_erosion, size)
    cv.imwrite(os.path.join(output_dir, f"erosion_{filename}"), img_erosion_resized)

def process_images_from_directory(directory, output_dir, folder_name):
    for subdir, dirs, files in os.walk(directory):
        for file in files:
            if file.endswith(".png"):
                img_path = os.path.join(subdir, file)
                filename = os.path.splitext(file)[0]
                resize_and_process_image(img_path, output_dir, f"{folder_name}_{filename}.png")

for folder_name in pastas:
    input_dir = os.path.join(directory, "Img", folder_name)
    output_dir = os.path.join(directory1, 'ds', folder_name)
    if not os.path.exists(output_dir):
        os.makedirs(output_dir)
    process_images_from_directory(input_dir, output_dir, folder_name)
