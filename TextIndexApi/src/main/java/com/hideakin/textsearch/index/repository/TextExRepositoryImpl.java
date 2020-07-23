package com.hideakin.textsearch.index.repository;

import java.util.List;

import javax.persistence.EntityManager;
import javax.persistence.PersistenceContext;

import org.springframework.stereotype.Repository;

import com.hideakin.textsearch.index.entity.TextEntity;

@Repository
public class TextExRepositoryImpl implements TextExRepository {

	@PersistenceContext
	private EntityManager em;

	@Override
	public List<TextEntity> findByTextContainingAndGid(String text, int gid, int limit, int offset) {
		return findPartialByLike(String.format("%%%s%%", text), gid, limit, offset);
	}

	@Override
	public List<TextEntity> findByTextStartingWithAndGid(String text, int gid, int limit, int offset) {
		return findPartialByLike(String.format("%s%%", text), gid, limit, offset);
	}

	@Override
	public List<TextEntity> findByTextEndingWithAndGid(String text, int gid, int limit, int offset) {
		return findPartialByLike(String.format("%%%s", text), gid, limit, offset);
	}

	@SuppressWarnings("unchecked")
	@Override
	public List<String> findTextByGid(int gid, int limit, int offset) {
		return (List<String>)em.createQuery("SELECT t.text FROM texts t WHERE t.gid = :gid ORDER BY t.text")
				.setParameter("gid", gid)
				.setMaxResults(limit)
				.setFirstResult(offset)
				.getResultList();
	}

	@SuppressWarnings("unchecked")
	private List<TextEntity> findPartialByLike(String expr, int gid, int limit, int offset) {
		return (List<TextEntity>)em.createQuery("SELECT t FROM texts t WHERE t.text LIKE :expr AND t.gid = :gid ORDER BY t.text")
				.setParameter("expr", expr)
				.setParameter("gid", gid)
				.setMaxResults(limit)
				.setFirstResult(offset)
				.getResultList();
	}

}
