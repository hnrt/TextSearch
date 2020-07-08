package com.hideakin.textsearch.index.repository;

import java.util.List;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import com.hideakin.textsearch.index.entity.TextEntity;

@Repository
public interface TextRepository extends JpaRepository<TextEntity,String> {

	TextEntity findByTextAndGid(String text, int gid);
	List<TextEntity> findAllByTextStartingWithAndGid(String text, int gid);
	List<TextEntity> findAllByTextEndingWithAndGid(String text, int gid);
	List<TextEntity> findAllByTextContainingAndGid(String text, int gid);
	void deleteByGid(int gid);

}
