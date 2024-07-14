from rest_framework import serializers
from SCTDB.models import Profile,Computer

class ProfileSerializer(serializers.Serializer):
    name = serializers.CharField(max_length=30)
    surname = serializers.CharField(max_length=30)
    patronymic = serializers.CharField(max_length=30)
    email = serializers.CharField(max_length=30,allow_blank=True)
    company = serializers.CharField(max_length=30)
    apply_date = serializers.DateField()
    fire_date = serializers.DateField(allow_null=True)
    appointment = serializers.CharField(max_length=30)
    city = serializers.CharField(max_length=30)
    profiles = serializers.ListField( child=serializers.CharField())
    img_src = serializers.CharField(max_length=150)


    def create(self, validated_data):
        return Profile.objects.create(**validated_data)
    
    def update(self, instance, validated_data):
        instance.name = validated_data.get('name',instance.name)
        instance.surname = validated_data.get('surname',instance.surname)
        instance.email = validated_data.get('email',instance.email)
        instance.company = validated_data.get('company',instance.company)
        instance.apply_date = validated_data.get('apply_date',instance.apply_date)
        instance.fire_date = validated_data.get('fire_date',instance.fire_date)
        instance.appointment = validated_data.get('appointment',instance.appointment)
        instance.city = validated_data.get('city',instance.city)
        instance.save()
        return instance
    
class ComputerSerializer(serializers.Serializer):
    WindowsEdition = serializers.CharField(max_length=80)
    IPAddress = serializers.CharField(max_length=20)
    DomainName = serializers.CharField(max_length=30)
    TotalRAMGB = serializers.IntegerField()
    DiskSpace = serializers.JSONField()
    CPUName = serializers.JSONField()
    CPUCores = serializers.JSONField()
    ComputerName = serializers.CharField(max_length=30)
    Status = serializers.BooleanField(allow_null=True)

    def create(self, validated_data):
        return Computer.objects.create(**validated_data)
    
    def update(self, instance, validated_data):
        instance.WindowsEdition = validated_data.get('WindowsEdition',instance.WindowsEdition)
        instance.IPAddress = validated_data.get('IPAddress',instance.IPAddress)
        instance.DomainName = validated_data.get('DomainName',instance.DomainName)
        instance.TotalRAMGB = validated_data.get('TotalRAMGB',instance.TotalRAMGB)
        instance.DiskSpace = validated_data.get('DiskSpace',instance.DiskSpace)
        instance.CPUName = validated_data.get('CPUName',instance.CPUName)
        instance.CPUCores = validated_data.get('CPUCores',instance.CPUCores)
        instance.ComputerName = validated_data.get('ComputerName',instance.ComputerName)
        instance.Status = validated_data.get('Status',instance.Status)
        instance.save()
        return instance    
    
class IdSerializer(serializers.Serializer):
    id = serializers.CharField(max_length=30)