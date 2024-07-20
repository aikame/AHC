from django.db import models
class EMPImage(models.Model):
    user_id = models.CharField(max_length=255, unique=True)
    avatar = models.ImageField(upload_to='employee_avatars/')