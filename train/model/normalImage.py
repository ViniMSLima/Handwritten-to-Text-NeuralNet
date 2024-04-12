import os
from tensorflow.keras import models, layers, activations, \
optimizers, utils, losses, initializers, metrics, callbacks, regularizers

epochs = 500 
batch_size = 32
patience = 10
learning_rate = 0.001
model_path = 'checkpoints/model.keras'
exists = os.path.exists(model_path)

model = models.load_model(model_path) if exists else models.Sequential([
    layers.Resizing(64, 64),
    layers.RandomRotation((-0.2, 0.2)),
    layers.Dropout(0.2),
    layers.Conv2D(16, (5, 5),
                    activation='relu',
                    kernel_initializer=initializers.RandomNormal()
                ),
    layers.MaxPooling2D((2, 2)),
    layers.Conv2D(64, (5, 5),
                    activation='relu',
                    kernel_initializer=initializers.RandomNormal()
                ),
    layers.MaxPooling2D((2, 2)),
    layers.Conv2D(64, (5, 5),
                    activation='relu',
                    kernel_initializer=initializers.RandomNormal()
                ),
    layers.MaxPooling2D((2, 2)),
    layers.Conv2D(128, (3, 3),
                    activation='relu',
                    kernel_initializer=initializers.RandomNormal()
                ),
    layers.MaxPooling2D((2, 2)),
    layers.Flatten(),
    layers.Dense(128,
                    activation='relu',
                    kernel_initializer=initializers.RandomNormal()
                ),
    layers.Dense(64,
                    activation='relu',
                    kernel_initializer=initializers.RandomNormal()
                ),
    layers.Dense(64,
                    activation='relu',
                    kernel_initializer=initializers.RandomNormal()
                ),
    layers.Dropout(0.2),
    layers.Dense(32,
                    activation='relu',
                    kernel_initializer=initializers.RandomNormal()
                ),
    layers.Dense(62,
                    activation='softmax',
                    kernel_initializer=initializers.RandomNormal()
                )
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
    image_size= (128, 128),
    batch_size= batch_size
)

test = utils.image_dataset_from_directory(
    "datasets/ds",
    validation_split= 0.2,
    subset= "validation",
    seed= 123,
    shuffle= True,
    image_size= (128, 128),
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
