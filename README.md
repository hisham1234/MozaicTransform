The Function applies blur effect to the detected faces and the text (Number Plates) in the uploaded picture in azure storage.

Azure Services which is used to detect the faces: Azure Face Service
Azure Service Which is used to detect the text in image :Optical Character Recognition

Azure Function will be triggered when a file is uploaded to the picture container in Azure file storage container. After successful mosaic conversion the picture will be uploaded to the blurred picture container in the same azure file storage.

We can specify the connection string for the file storage, endpoint and the subscription keys and the names of the specified containers in the configuration file. We can update the configuration file any time to change the azure storage or the containers.

Limitations in this azure function
The services which I have used to detect the face can sometimes fail to detect the faces which are tool small or faces already covered by a mask. It depends on the APIs behaviour.
