import os
from tensorflow.keras import models, layers, activations, \
optimizers, utils, losses, initializers, metrics, callbacks

epochs = 500 
batch_size = 64 
patience = 20
learning_rate = 0.0005
model_path = 'checkpoints/model.keras'
exists = os.path.exists(model_path)

model = models.load_model(model_path) if exists else models.Sequential([
    layers.Resizing(120, 120),
    layers.Rescaling(1.0 / 255),
    layers.RandomFlip(mode="horizontal_and_vertical", seed=120),
    layers.RandomRotation((-0.2, 0.2)),
    layers.Conv2D(16, (7, 7),
                  activation='relu',
                  kernel_initializer=initializers.RandomNormal()),
    layers.MaxPooling2D((2, 2)),
    layers.Conv2D(64, (5, 5),
                  activation='relu',
                  kernel_initializer=initializers.RandomNormal()),
    layers.MaxPooling2D((2, 2)),
    layers.Conv2D(128, (3, 3),
                  activation='relu',
                  kernel_initializer=initializers.RandomNormal()),
    layers.MaxPooling2D((2, 2)),
    layers.Dropout(0.2),
    layers.Flatten(),
    layers.Dense(128,
                 activation='relu',
                 kernel_initializer=initializers.RandomNormal()),
    layers.Dropout(0.5),
    layers.Dense(256,
                 activation='relu',
                 kernel_initializer=initializers.RandomNormal()),
    layers.Dropout(0.3),
    layers.Dense(128,
                 activation='relu',
                 kernel_initializer=initializers.RandomNormal()),
    layers.Dropout(0.2),
    layers.Dense(64,
                 activation='relu',
                 kernel_initializer=initializers.RandomNormal()),
    layers.Dense(32,
                 activation='relu',
                 kernel_initializer=initializers.RandomNormal()),
    layers.Dense(17,
                 activation='softmax',
                 kernel_initializer=initializers.RandomNormal())
])
    
if exists:
    model.summary()
else:
    model.compile(
        optimizer = optimizers.Adam(
            learning_rate = learning_rate
        ),
        loss = losses.SparseCategoricalCrossentropy(),
        metrics = [ 'accuracy' ]
    )
    
train = utils.image_dataset_from_directory(
    "datasets/ds",
    validation_split= 0.2,
    subset= "training",
    seed= 123,
    shuffle= True,
    image_size= (1200, 900),
    batch_size= batch_size
)

test = utils.image_dataset_from_directory(
    "datasets/ds",
    validation_split= 0.2,
    subset= "validation",
    seed= 123,
    shuffle= True,
    image_size= (1200, 900),
    batch_size= batch_size
)

model.fit(train,
    epochs = epochs,
    validation_data = test,
    callbacks= [
        callbacks.EarlyStopping(
            monitor = 'val_loss',
            patience = patience,
            verbose = 1
        ),
        callbacks.ModelCheckpoint(
            filepath = model_path,
            save_weights_only = False,
            monitor = 'loss',
            mode = 'min',
            save_best_only = True
        )
    ]
)
