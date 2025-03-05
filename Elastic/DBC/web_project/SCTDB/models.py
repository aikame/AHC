from django.db import models
# Create your models here.
class Profile(models.Model):
    created = models.DateTimeField(auto_now_add=True)
    name = models.CharField(max_length=30)
    surname = models.CharField(max_length=30)
    patronymic = models.CharField(max_length=30)
    email = models.CharField(max_length=30,blank=True)
    company = models.CharField(max_length=30)
    apply_date = models.DateField()
    fire_date = models.DateField(blank=True)
    appointment = models.CharField(max_length=30)
    city = models.CharField(max_length=30)
    profiles = models.CharField(max_length=1024)
    img_src = models.CharField(max_length=150)
    class Meta:
        ordering = ['created']

class Computer(models.Model):
    updated = models.DateTimeField(auto_now_add=True)
    WindowsEdition = models.CharField(max_length=80)
    IPAddress = models.CharField(max_length=20)
    DomainName = models.CharField(max_length=30)
    TotalRAMGB = models.IntegerField()
    DiskSpace = models.JSONField(max_length=500)
    CPUName = models.JSONField(max_length=500)
    CPUCores = models.JSONField(max_length=50)
    ComputerName = models.CharField(max_length=30)
    ComputerRole = models.IntegerField()
    Status = models.BooleanField()
    class Meta:
        ordering = ['updated']

class Group(models.Model):
    updated = models.DateTimeField(auto_now_add=True)
    PSComputerName = models.CharField(max_length=100)
    Description = models.CharField(max_length=200)
    RunspaceId = models.CharField(max_length=100)
    DistinguishedName = models.CharField(max_length=100)
    GroupCategory = models.CharField(max_length=20)
    GroupScope = models.CharField(max_length=20)
    Name = models.CharField(max_length=100)
    ObjectClass = models.CharField(max_length=20)
    ObjectGUID = models.CharField(max_length=100)
    SamAccountName = models.CharField(max_length=100)
    SID = models.CharField(max_length=100)
    class Meta:
        ordering = ['updated']