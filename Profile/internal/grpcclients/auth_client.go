package grpcclients

import (
	"context"
	"crypto/tls"
	"crypto/x509"
	"log"
	"os"
	"time"

	"profile/internal/config"
	pb "profile/protos/auth"

	"google.golang.org/grpc"
	"google.golang.org/grpc/credentials"
)

type AuthClient interface {
	Test(message string) (*pb.TestReply, error)
	ValidateToken(token string) (*pb.TokenValidationReply, error)
}

type authClientImpl struct {
	client pb.AuthorizationServiceClient
}

func NewAuthClient(grpcAddr string, cfg *config.Config) (AuthClient, *grpc.ClientConn) {
	myCert, err := tls.LoadX509KeyPair(cfg.Grpc.Client_cert_crt, cfg.Grpc.Client_cert_key)
	if err != nil {
		log.Fatalf("Could not load client key pair: %v", err)
	}
	cert, err := os.ReadFile(cfg.Grpc.Server_cert_ca)
	if err != nil {
		log.Fatalf("Could not read cert file: %v", err)
	}

	certPool := x509.NewCertPool()
	if !certPool.AppendCertsFromPEM(cert) {
		log.Fatalf("Failed to append server certificate to pool")
	}

	tlsCfg := &tls.Config{
		Certificates:       []tls.Certificate{myCert},
		RootCAs:            certPool,
		InsecureSkipVerify: false,
		ServerName:         cfg.Grpc.Server,
	}
	creds := credentials.NewTLS(tlsCfg)
	conn, err := grpc.NewClient(grpcAddr, grpc.WithTransportCredentials(creds))
	if err != nil {
		log.Fatalf("Failed to connect to gRPC server: %v", err)
	}
	client := pb.NewAuthorizationServiceClient(conn)
	return &authClientImpl{client: client}, conn
}

func (a *authClientImpl) Test(message string) (*pb.TestReply, error) {
	ctx, cancel := context.WithTimeout(context.Background(), time.Second*2)
	defer cancel()

	return a.client.TestFunction(ctx, &pb.TestMessage{
		TestField: message,
	})
}

func (a *authClientImpl) ValidateToken(token string) (*pb.TokenValidationReply, error) {
	ctx, cancel := context.WithTimeout(context.Background(), time.Second*2)
	defer cancel()
	return a.client.ValidateToken(ctx, &pb.TokenRequest{
		Token: token,
	})
}
