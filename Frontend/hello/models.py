from django.db import models
class EMPImage(models.Model):
    title = models.CharField(max_length=200)
    image = models.ImageField(upload_to='employee_avatars')
    def __str__(self):
        return self.title