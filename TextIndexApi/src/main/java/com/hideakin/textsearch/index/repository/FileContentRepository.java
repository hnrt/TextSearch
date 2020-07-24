package com.hideakin.textsearch.index.repository;

import java.util.Collection;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import com.hideakin.textsearch.index.entity.FileContentEntity;

@Repository
public interface FileContentRepository extends JpaRepository<FileContentEntity,Integer> {

	FileContentEntity findByFid(int fid);
	void deleteByFid(int fid);
	void deleteByFidIn(Collection<Integer> fids);

}
