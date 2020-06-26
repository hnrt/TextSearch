package com.hideakin.textsearch.index.repository;

import java.util.List;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import com.hideakin.textsearch.index.entity.UserEntity;

@Repository
public interface UserRepository extends JpaRepository<UserEntity,Integer>  {

	public UserEntity findByUid(int uid);
	public List<UserEntity> findAllByUsername(String username);
	public List<UserEntity> findAllByApiKey(String apiKey);

}
