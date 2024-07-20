from django import forms
from .models import EMPImage

class UploadFileForm(forms.Form):
        fields = ['avatar']