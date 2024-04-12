import numpy as np
import matplotlib.pyplot as plt
import cv2 as cv
import os
import string
from random import randint

directory = f'datasets/'
directory1 = f'datasets/'
numeros = [str(i) for i in range(10)]
letras  = [("upper" + chr(i))  for i in range(65, 91)]
letras_minusculas = [("lower" + chr(i)) for i in range(65, 91)]

pastas = numeros + letras + letras_minusculas

exist = os.path.exists(f'datasets/ds/0')

if not exist:
    for pst in pastas:
        os.mkdir(f"datasets/ds/{pst}")

def show(img):
    plt.imshow(img, cmap = 'gray')
    plt.show()
    return img

kernel_dilation = np.ones((40, 40), np.uint8)
kernel_erosion = np.ones((25, 25), np.uint8)

def read_images_from_directory(directory, pastinha):
    i = 0
    for subdir, dirs, files in os.walk(directory):
        for file in files:
            if file.endswith(".png"):
                img_path = os.path.join(subdir, file)
                img = cv.imread(img_path, cv.COLOR_BGRA2GRAY)
                rand = randint(0, 10)
                
                if rand % 2 == 0:
                    img_dilation = cv.dilate(img, kernel_dilation)
                    cv.imwrite(os.path.join(f"{directory1}", 'ds/', f"{pastinha}/",f"{i}.png"), img_dilation)
                    i += 1
                    
                else:
                    img_erosion = cv.erode(img, kernel_erosion)
                    cv.imwrite(os.path.join(f"{directory1}", 'ds/', f"{pastinha}/", f"{i}.png"), img_erosion)
                    i += 1

for i in range(len(pastas)):
    dir = os.path.join(f"{directory}", "Img/", pastas[i])
    read_images_from_directory(os.path.join(dir), pastas[i])
