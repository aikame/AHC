from rest_framework import serializers
from SCTDB.models import Profile

class ProfileSerializer(serializers.Serializer):
    name = serializers.CharField(max_length=30)
    surname = serializers.CharField(max_length=30)
    patronymic = serializers.CharField(max_length=30)
    email = serializers.CharField(max_length=30, allow_blank=True)
    company = serializers.CharField(max_length=30)
    apply_date = serializers.DateField()
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
        instance.appointment = validated_data.get('appointment',instance.appointment)
        instance.city = validated_data.get('city',instance.city)
        instance.save()
        return instance
    
class IdSerializer(serializers.Serializer):
    id = serializers.CharField(max_length=30)