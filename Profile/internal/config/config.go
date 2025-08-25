package config

import (
	"log"

	"github.com/spf13/viper"
)

type Config struct {
	API   APIConfig
	Grpc  GrpcConfig
	Neo4j Neo4jConfig
}

type APIConfig struct {
	Server                string
	Port                  int
	Authentication_secret string
	Client_cert_key       string
	Client_cert_crt       string
	Claims_role           string
	Claims_id             string
}

type GrpcConfig struct {
	Server          string
	Port            int
	Client_cert_key string
	Client_cert_crt string
	Server_cert_ca  string
}

type Neo4jConfig struct {
	DbUri      string
	DbUser     string
	DbPassword string
	DbName     string
}

func Load() *Config {
	viper.SetConfigName("config")
	viper.SetConfigType("json")
	viper.AddConfigPath(".")
	viper.AutomaticEnv()

	if err := viper.ReadInConfig(); err != nil {
		log.Fatalf("Error reading config file %s", err)
	}
	var cfg Config
	if err := viper.Unmarshal(&cfg); err != nil {
		log.Fatalf("Unable to decode into struct: %v", err)
	}
	return &cfg
}
