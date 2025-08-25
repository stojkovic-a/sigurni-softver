package services

import (
	"context"
	"log"
	"profile/internal/config"
	"profile/internal/models"

	"github.com/neo4j/neo4j-go-driver/v5/neo4j"
)

type Neo4jRepoImpl struct {
	cfg    *config.Config
	driver *neo4j.DriverWithContext
	ctx    *context.Context
}

func (n *Neo4jRepoImpl) GetProfileById(id *string) (*models.Profile, error) {
	result, err := neo4j.ExecuteQuery(*n.ctx, *n.driver,
		`MATCH (p:Profile {userId:$id})
	RETURN p`,
		map[string]any{
			"id": id,
		}, neo4j.EagerResultTransformer,
		neo4j.ExecuteQueryWithDatabase(n.cfg.Neo4j.DbName))

	if err != nil {
		return nil, err
	}

	if len(result.Records) == 0 {
		return nil, nil
	}
	node := (*result).Records[0].Values[0].(neo4j.Node)
	props := node.Props
	return &models.Profile{
		UserId:          props["userId"].(string),
		UserName:        props["username"].(string),
		Name:            props["name"].(string),
		DateOfBirth:     props["dateOfBirth"].(string),
		ProfilePhotoUrl: props["profilePhotoUrl"].(string),
	}, nil
}

func (n *Neo4jRepoImpl) GetProfileByUsername(username *string) (*models.Profile, error) {
	result, err := neo4j.ExecuteQuery(*n.ctx, *n.driver,
		`MATCH (p:Profile {username:$username})
	RETURN p`,
		map[string]any{
			"username": username,
		}, neo4j.EagerResultTransformer,
		neo4j.ExecuteQueryWithDatabase(n.cfg.Neo4j.DbName))

	if err != nil {
		return nil, err
	}
	if len(result.Records) == 0 {
		return nil, nil
	}
	node := (*result).Records[0].Values[0].(neo4j.Node)
	props := node.Props
	return &models.Profile{
		UserId:          props["userId"].(string),
		UserName:        props["username"].(string),
		Name:            props["name"].(string),
		DateOfBirth:     props["dateOfBirth"].(string),
		ProfilePhotoUrl: props["profilePhotoUrl"].(string),
	}, nil
}

func (n *Neo4jRepoImpl) AddFriend(f1 *string, f2 *string) error {
	_, err := neo4j.ExecuteQuery(*n.ctx, *n.driver,
		`MATCH (a:Profile {userId:$id1}), (b:Profile {userId:$id2})
		MERGE (a)-[:FRIENDS]->(b)
		MERGE (b)-[:FRIENDS]->(a)`,
		map[string]any{
			"id1": *f1,
			"id2": *f2,
		},
		neo4j.EagerResultTransformer,
		neo4j.ExecuteQueryWithDatabase(n.cfg.Neo4j.DbName))
	return err
}

func (n *Neo4jRepoImpl) CreateProfile(p *models.Profile) error {
	_, err := neo4j.ExecuteQuery(*n.ctx, *n.driver,
		`CREATE (a:Profile{
			userId:$id,
			username:$username,
			name:$name,
			dateOfBirth:$dateOfBirth,
			profilePhotoUrl:$profilePhotoUrl
		})`,
		map[string]any{
			"name":            p.Name,
			"username":        p.UserName,
			"id":              p.UserId,
			"dateOfBirth":     p.DateOfBirth,
			"profilePhotoUrl": p.ProfilePhotoUrl,
		},
		neo4j.EagerResultTransformer,
		neo4j.ExecuteQueryWithDatabase(n.cfg.Neo4j.DbName))
	return err

}

func (n *Neo4jRepoImpl) DeleteProfile(id *string) error {
	_, err := neo4j.ExecuteQuery(*n.ctx, *n.driver,
		`MATCH (p:Profile {userId:$id})
	DETACH DELETE p`,
		map[string]any{
			"id": id,
		},
		neo4j.EagerResultTransformer,
		neo4j.ExecuteQueryWithDatabase(n.cfg.Neo4j.DbName),
	)
	return err
}

func (n *Neo4jRepoImpl) Unfriend(id1 *string, id2 *string) error {
	_, err := neo4j.ExecuteQuery(*n.ctx, *n.driver,
		`MATCH (a:Profile {userId: $id1})-[r1:FRIENDS]->(b:Profile {userId: $id2})
	DELETE r1
	MATCH (b)-[r2:FRIENDS]->(a)
	DELETE r2`,
		map[string]any{
			"id1": id1,
			"id2": id2,
		},
		neo4j.EagerResultTransformer,
		neo4j.ExecuteQueryWithDatabase(n.cfg.Neo4j.DbName),
	)
	return err
}
func (n *Neo4jRepoImpl) Close() error {
	err := (*n.driver).Close(*n.ctx)
	return err
}
func NewNeo4jService(
	cfg *config.Config,
) Neo4jRepo {
	ctx := context.Background()
	dbUri := cfg.Neo4j.DbUri
	dbUser := cfg.Neo4j.DbUser
	dbPassword := cfg.Neo4j.DbPassword
	driver, err := neo4j.NewDriverWithContext(
		dbUri, neo4j.BasicAuth(dbUser, dbPassword, ""),
	)
	if err != nil {
		log.Fatalf("Failed to connect to Neo4j %v", err)
	}
	if err = driver.VerifyConnectivity(ctx); err != nil {
		log.Fatalf("Neo4j connection verification failed %v", err)
	}

	return &Neo4jRepoImpl{
		cfg:    cfg,
		driver: &driver,
		ctx:    &ctx,
	}
}
