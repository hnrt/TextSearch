package com.hideakin.textsearch.index.service;

import java.util.HashMap;
import java.util.List;
import java.util.Map;

import javax.persistence.EntityManager;
import javax.persistence.PersistenceContext;
import javax.transaction.Transactional;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import com.hideakin.textsearch.index.data.SearchOptions;
import com.hideakin.textsearch.index.entity.FileGroupEntity;
import com.hideakin.textsearch.index.entity.TextEntity;
import com.hideakin.textsearch.index.exception.InvalidParameterException;
import com.hideakin.textsearch.index.model.TextDistribution;
import com.hideakin.textsearch.index.repository.FileGroupRepository;
import com.hideakin.textsearch.index.repository.TextRepository;

@Service
@Transactional
public class IndexServiceImpl implements IndexService {

	@PersistenceContext
	private EntityManager em;

	@Autowired
	private FileGroupRepository fileGroupRepository;

	@Autowired
	private TextRepository textRepository;

	@Override
	public TextDistribution[] findText(String group, String text, SearchOptions option, int limit, int offset) {
		FileGroupEntity fileGroupEntity = fileGroupRepository.findByName(group);
		if (fileGroupEntity == null) {
			return null;
		}
		int gid = fileGroupEntity.getGid();
		Map<Integer, TextDistribution> map = new HashMap<Integer, TextDistribution>();
		if (option == SearchOptions.Exact) {
			TextEntity textEntity = textRepository.findByTextAndGid(text, gid);
			if (textEntity == null) {
				return new TextDistribution[0];
			}
			populateHitMap(map, textEntity);
		} else {
			if (limit <= 0) {
				throw new InvalidParameterException("invalid_limit", "limit needs to be greater than 0.");
			}
			if (offset < 0) {
				throw new InvalidParameterException("invalid_offset", "offset needs to be equal to or greater than 0.");
			}
			List<TextEntity> textEntities;
			if (option == SearchOptions.Contains) {
				textEntities = findPartialByTextContainingAndGid(text, gid, limit, offset);
			} else if (option == SearchOptions.StartsWith) {
				textEntities = findPartialByTextStartingWithAndGid(text, gid, limit, offset);
			} else if (option == SearchOptions.EndsWith) {
				textEntities = findPartialByTextEndingWithAndGid(text, gid, limit, offset);
			} else {
				return new TextDistribution[0]; // never reach here
			}
			populateHitMap(map, textEntities);
		}
		return map.values().toArray(new TextDistribution[map.size()]);
	}

	private void populateHitMap(Map<Integer, TextDistribution> map, List<TextEntity> entities) {
		for (TextEntity entity : entities) {
			populateHitMap(map, entity);
		}
	}

	private void populateHitMap(Map<Integer, TextDistribution> map, TextEntity textEntity) {
		TextDistribution.PackedSequence sequence = TextDistribution.sequence(textEntity.getDist());
		for (TextDistribution dist = sequence.get(); dist != null; dist = sequence.get()) {
			int fid = dist.getFid();
			TextDistribution stored = map.get(fid);
			if (stored != null) {
				stored.addPositions(dist.getPositions());
			} else {
				map.put(fid, dist);
			}
		}
	}

	private List<TextEntity> findPartialByTextContainingAndGid(String text, int gid, int limit, int offset) {
		return findPartialByLike(String.format("%%%s%%", text), gid, limit, offset);
	}

	private List<TextEntity> findPartialByTextStartingWithAndGid(String text, int gid, int limit, int offset) {
		return findPartialByLike(String.format("%s%%", text), gid, limit, offset);
	}

	private List<TextEntity> findPartialByTextEndingWithAndGid(String text, int gid, int limit, int offset) {
		return findPartialByLike(String.format("%%%s", text), gid, limit, offset);
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
