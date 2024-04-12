import os
import cv2 as cv
import numpy as np
import matplotlib.pyplot as plt
from random import randint
from PIL import Image

def resizeImage(image_path, output_path, size=(128, 128)):
    img = Image.open(image_path)
    img_resized = img.resize(size)
    img_resized.save(output_path)

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

def read_images_from_directory(directory, pastinha):
    i = 0
    for subdir, dirs, files in os.walk(directory):
        for file in files:
            if file.endswith(".png"):
                img_path = os.path.join(subdir, file)
                img = cv.imread(img_path, cv.IMREAD_GRAYSCALE)
                rand = randint(0, 10)
                temp_img_path = os.path.join(directory1, 'ds/', f"{pastinha}/", f"{i}.png")

                if rand % 2 == 0:
                    img_dilation = cv.dilate(img, kernel_dilation)
                    cv.imwrite(temp_img_path, img_dilation)
                else:
                    img_erosion = cv.erode(img, kernel_erosion)
                    cv.imwrite(temp_img_path, img_erosion)

                resized_img_path = os.path.join(directory1, 'ds/', f"{pastinha}/", f"{i}.png")
                resizeImage(temp_img_path, resized_img_path)
                i += 1

for pastinha in pastas:
    dir = os.path.join(f"{directory}", "Img/", pastinha)
    read_images_from_directory(dir, pastinha)
