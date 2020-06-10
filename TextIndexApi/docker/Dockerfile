FROM centos:7
RUN yum install -y java
COPY TextIndexApi-0.0.2-SNAPSHOT.jar /opt/
CMD [ "java", "-jar", "/opt/TextIndexApi-0.0.2-SNAPSHOT.jar" ]
