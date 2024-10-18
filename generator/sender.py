import pika
import os
import time

class Sender:

    def __init__(self, queue_name):
        self.__host = os.getenv('RABBITMQ_HOST')
        self.__queue_name = queue_name
        self.__connection = None
        self.__channel = None
        self.__username = os.getenv('USERNAME')
        self.__password = os.getenv('PASSWORD')

    def connect_to_queue(self):
        try:
            credentials = pika.PlainCredentials(
                self.__username,
                self.__password)
            connection_params = pika.ConnectionParameters(
                host=self.__host,
                credentials=credentials)
            self.__connection = pika.BlockingConnection(connection_params)
            self.__channel = self.__connection.channel()
            self.__channel.queue_declare(
                queue=self.__queue_name,
                durable=True)
            print("Connected to RabbitMQ")
        except Exception as e:
            print(f"Failed to connect to RabbitMQ: {e}")

    def send(self, body):
        try:
            self.__channel.basic_publish(
                exchange='',
                routing_key=self.__queue_name,
                body=body,
                properties=pika.BasicProperties(
                    delivery_mode=2,  # Make message persistent
                )
            )
        except Exception as e:
            print(f"Failed to send message: {e}")

    def close(self):
        if self.__connection:
            self.__connection.close()
            print("Connection to RabbitMQ closed.")
