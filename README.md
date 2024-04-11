# Handwritten Character and Number Recognition using Keras Neural Network

## Introduction
This Python project aims to recognize handwritten characters and numbers from a dataset using a neural network implemented with Keras. Handwritten character and number recognition is a fundamental problem in computer vision and machine learning, with applications ranging from optical character recognition (OCR) to automated form processing.

## Requirements
- Python 3.x
- Keras
- NumPy
- Matplotlib (for visualization, optional)
- [HandWritten Dataset] (https://www.kaggle.com/datasets/dhruvildave/englishhandwritten-characters-dataset/data)

## Installation
1. **Clone this repository to your local machine:**  
    ```bash
    git clone https://github.com/your_username/handwritten-recognition.git
    ```

2. **Install the required dependencies:**
    ```bash
    pip install keras numpy matplotlib
    ```
3. **Download the dataset and place it in the datasets/ directory.**

## Usage

1. **Navigate to the project directory:** 
    ```bash
    cd handwritten-recognition
    ```

2. **Use the transform.py to apply filters on the dataset images randomly:**
    ```bash
    python transform.py
    ```

2. **Train the neural network:**
    ```bash
    python train.py
    ```

3. **Once training is complete, test the model:**
    ```bash
    python test.py
    ```

## Dataset

The dataset used in this project consists of handwritten characters and numbers. It contains a total of 3,410 samples, where each sample is a grayscale image of size YxZ pixels. It contains 62 classes with 55 images of each class. The 62 classes are 0-9, A-Z and a-z.

Dataset: [HandWritten Dataset] (https://www.kaggle.com/datasets/dhruvildave/english-handwritten-characters-dataset/data).